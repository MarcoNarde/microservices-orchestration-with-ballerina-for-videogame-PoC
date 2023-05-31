import argparse
import subprocess
import time
from concurrent.futures import ThreadPoolExecutor, as_completed

# Funzione per inviare una richiesta CURL e controllare il risultato
def send_request(url):
    try:
        # Esegui il comando CURL
        result = subprocess.run(['curl', '-s', '-o', '/dev/null', '-w', '%{http_code}', url], capture_output=True, text=True)

        # Controlla il codice di stato HTTP
        return result.stdout.strip() == "200"
    except Exception as e:
        print(f"Errore durante l'invio della richiesta a {url}: {str(e)}")
        return False

# Funzione per eseguire il test multiple volte
def run_test(url, num_requests, num_iterations, num_threads):
    total_successful_requests = 0
    total_failed_requests = 0

    for iteration in range(num_iterations):
        print(f"Iteration: {iteration + 1}")

        # Lista delle URL di richiesta con il numero della richiesta
        urls = [url + str(i) for i in range(num_requests)]

        # Esecuzione concorrente delle richieste
        with ThreadPoolExecutor(max_workers=num_threads) as executor:
            # Lista di oggetti Future
            futures = [executor.submit(send_request, url) for url in urls]

            # Contatori delle richieste soddisfatte correttamente e tempi di risposta
            successful_requests = 0

            # Monitora gli oggetti Future completati
            for future in as_completed(futures):
                result = future.result()
                if result:
                    successful_requests += 1

            # Calcola il numero di richieste fallite
            failed_requests = num_requests - successful_requests

            # Aggiorna i totali delle richieste
            total_successful_requests += successful_requests
            total_failed_requests += failed_requests

            # Attendere il completamento di tutte le richieste
            executor.shutdown(wait=True)

        # Stampa il risultato per l'iterazione corrente
        print(f"Numero totale di richieste: {num_requests}")
        print(f"Numero di richieste soddisfatte correttamente: {successful_requests}")
        print(f"Numero di richieste fallite: {failed_requests}")
        print()

        # Attendere prima di passare all'iterazione successiva
        time.sleep(1)

    # Calcola la media di successo e di insuccesso delle risposte
    average_success_rate = (total_successful_requests / (num_requests * num_iterations)) * 100
    average_failure_rate = (total_failed_requests / (num_requests * num_iterations)) * 100

    # Stampa la media di successo e di insuccesso delle risposte
    print(f"Media di successo delle risposte: {average_success_rate}%")
    print(f"Media di insuccesso delle risposte: {average_failure_rate}%")

# Parsing degli argomenti dalla CLI
parser = argparse.ArgumentParser(description='Script di test per le richieste CURL.')
parser.add_argument('url', type=str, help='URL a cui inviare la richiesta CURL')
parser.add_argument('num_requests', type=int, help='Numero di richieste da inviare')
parser.add_argument('num_iterations', type=int, help='Numero di iterazioni')
parser.add_argument('--num_threads', type=int, default=10, help='Numero di thread da utilizzare per l\'esecuzione concorrente (predefinito: 10)')
args = parser.parse_args()

# URL base del microservizio da testare
microservice_url = args.url

# Numero di richieste da inviare
num_requests = args.num_requests

# Numero di iterazioni
num_iterations = args.num_iterations

# Numero di thread da utilizzare per l'esecuzione concorrente
num_threads = args.num_threads

# Esecuzione del test
run_test(microservice_url, num_requests, num_iterations, num_threads)
