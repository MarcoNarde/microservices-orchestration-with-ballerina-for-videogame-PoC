import argparse
import subprocess
import time
from concurrent.futures import ThreadPoolExecutor, as_completed

# Funzione per inviare una richiesta CURL e controllare il risultato
def send_request(url):
    try:
        # Esegui il comando CURL
        start_time = time.time()
        result = subprocess.run(['curl', '-s', '-o', '/dev/null', '-w', '%{http_code}', url], capture_output=True, text=True)
        end_time = time.time()

        # Controlla il codice di stato HTTP
        if result.stdout.strip() == "200": # result.returncode == 0 and
            return True, (end_time - start_time) * 1000
        else:
            return False, (end_time - start_time) * 1000
    except Exception as e:
        print(f"Errore durante l'invio della richiesta a {url}: {str(e)}")
        return False, 0

# Parsing degli argomenti dalla CLI
parser = argparse.ArgumentParser(description='Script di test per le richieste CURL.')
parser.add_argument('url', type=str, help='URL a cui inviare la richiesta CURL')
parser.add_argument('num_requests', type=int, help='Numero di richieste da inviare')
parser.add_argument('--num_threads', type=int, default=10, help='Numero di thread da utilizzare per l\'esecuzione concorrente (predefinito: 10)')
args = parser.parse_args()

# URL base del microservizio da testare
microservice_url = args.url

# Numero di richieste da inviare
num_requests = args.num_requests

# Lista delle URL di richiesta con il numero della richiesta
urls = [microservice_url + str(i) for i in range(num_requests)]

# Numero di thread da utilizzare per l'esecuzione concorrente
num_threads = args.num_threads

# Esecuzione concorrente delle richieste
with ThreadPoolExecutor(max_workers=num_threads) as executor:
    # Lista di oggetti Future
    futures = [executor.submit(send_request, url) for url in urls]

    # Contatori delle richieste soddisfatte correttamente e tempi di risposta
    successful_requests = 0
    total_response_time = 0

    # Monitora gli oggetti Future completati
    for future in as_completed(futures):
        result, response_time = future.result()
        if result:
            successful_requests += 1
        total_response_time += response_time

# Calcola il tempo medio di risposta per ogni richiesta in millisecondi
average_response_time = total_response_time / num_requests if num_requests > 0 else 0

# Tempo totale per soddisfare tutte le richieste in secondi
total_response_time_sec = total_response_time

# Stampa il risultato
print(f"Numero totale di richieste: {num_requests}")
print(f"Numero di richieste soddisfatte correttamente: {successful_requests}")
print(f"Percentuale di successo: {(successful_requests / num_requests) * 100}%")
print(f"Tempo medio di risposta per ogni richiesta: {average_response_time} millisecondi")
print(f"Tempo totale per soddisfare tutte le richieste in sequenza: {total_response_time_sec / 1000} secondi")