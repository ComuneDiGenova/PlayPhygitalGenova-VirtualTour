<?php

namespace Drupal\phygital\Controller;

use Drupal\Core\Controller\ControllerBase;
use Symfony\Component\HttpFoundation\Request;
use Symfony\Component\HttpFoundation\Session\SessionInterface;
use Symfony\Component\HttpFoundation\JsonResponse;
use Drupal\Core\File\FileSystemInterface;
use Drupal\Core\Site\Settings;

use GuzzleHttp\Client;
use GuzzleHttp\RequestOptions;
use Symfony\Component\HttpFoundation\RedirectResponse;
use Drupal\Core\Url;

class PhygitalController extends ControllerBase
{
  public function index(Request $request)
  {
    $xx = "";
    $query = $request->query;
    if ($query->has('nome')) {
      $nome = $query->get('nome');
      $markup = '<p>Nome: ' . $nome . '</p>';
    } else {
      $markup = '<p class="red">Il parametro "nome" non è stato fornito nella query string.</p>';
    }

    // Effettua la chiamata GET con Basic Authentication
    /*
    $url='https://testnew.visitgenoa.it/jsonapi/api-poi-list';
    $client = new Client([
      RequestOptions::VERIFY => false,
      RequestOptions::HEADERS => [
        'Authorization' => 'Basic ' . base64_encode($username . ':' . $password),
      ],
    ]);
    $response = $client->get($url, [
      RequestOptions::VERIFY => false,
    ]);

    $content = $response->getBody()->getContents();
    $jsonData = json_decode($content, true);
    $session = \Drupal::service('session');
    $session->set('my_variable',$jsonData);
    */
    // termina chiamata GET

    return array(
      // '#type' => 'markup',
      '#markup' => $markup . "<br><div id='myTextarea'>text</div>",
      '#theme' => 'phygital_custom_page',
      '#attached' => [
        'library' => [
          'phygital/leafletlib',
          'phygital/phygital_libraries',
        ],
      ],
      '#test_var' => $this->t('Test Value'),
    );
  }

  public function csvToJson()
  {
    // Path al file CSV
    $csvFilePath = 'public://estrazione_punti_bbs.csv'; // Sostituisci con il percorso al tuo file CSV.
    // Path al file JSON nella cartella pubblica
    $jsonFilePath = 'public://estrazione_punti_bbs.json'; // Sostituisci con il percorso desiderato per il file JSON di output.

    // Leggi il CSV con campi separati da tabulazioni e convertilo in JSON.
    if (($handle = fopen($csvFilePath, 'r')) !== FALSE) {
      $jsonData = array();
      $header = fgetcsv($handle, 0, "\t"); // Ottieni i nomi delle colonne dalla prima riga
      $progressiveId = 1;
      while (($data = fgetcsv($handle, 0, "\t")) !== FALSE) {
        $row = array();
        $allowed = array();
        $row['Id'] = $progressiveId;
        $progressiveId++;
        foreach ($header as $index => $columnName) {
          // Rimuovi le virgolette dagli elementi e sostituisci la virgola con il punto per i valori numerici.

          if (in_array($columnName, ['X', 'Y', 'Filename'])) {
            $value = str_replace('"', '', $data[$index]);
            $value = str_replace(',', '.', $value);
            $row[$columnName] = $value;
          }
          /*
          $value = str_replace('"', '', $data[$index]);
          $value = str_replace(',', '.', $value);
          $row[$columnName] = $value;
*/
        }


        $jsonData[] = $row;
      }
      fclose($handle);
      // Converti l'array in JSON.
      // $jsonString = json_encode($jsonData, JSON_PRETTY_PRINT);

      $data = [
        'versione' => '1.0',
        'Base_url' => '',
        'sferiche' => $jsonData, // I dati ottenuti dal CSV
      ];
      $jsonString = json_encode($data, JSON_PRETTY_PRINT);

      // Salva il JSON nella cartella pubblica.
      $temporaryFilePath = 'temporary://example.json';
      file_save_data($jsonString, $temporaryFilePath, FileSystemInterface::EXISTS_REPLACE);

      // Ottieni il servizio file_system.
      $fileSystem = \Drupal::service('file_system');
      // Sposta il file temporaneo nella cartella pubblica.
      $fileSystem->move($temporaryFilePath, $jsonFilePath, FileSystemInterface::EXISTS_REPLACE);

      // Verifica se il file JSON è stato salvato correttamente.
      if ($fileSystem->realpath($jsonFilePath)) {
        // Ottieni l'URL del file JSON creato.
        $jsonFileUrl = file_create_url($jsonFilePath);

        // Restituisci la risposta JSON con l'URL del file JSON.
        return new JsonResponse(['url' => $jsonFileUrl]);
      } else {
        return new JsonResponse(['error' => 'Errore durante il salvataggio del file JSON nella cartella pubblica.']);
      }
    } else {
      return new JsonResponse(['error' => 'Impossibile aprire il file CSV.']);
    }
  }

  /**
   * Opens the WebGL application.
   *
   * @return \Symfony\Component\HttpFoundation\RedirectResponse
   *   The redirect response.
   */
  public function openWebGLApp()
  {
    // Parameters to pass to the WebGL application (example parameters).
    $param1 = 'value1';
    $param2 = 'value2';

    // Build the query string with the parameters.
    $query = [
      'param1' => $param1,
      'param2' => $param2,
      'timestamp' => time(),
    ];

    // Create the URL for the WebGL application with the query string.
    $url = Url::fromRoute('phygital.open_webgl_app', [], ['query' => $query]);

    // Get the string representation of the URL.
    $destination = $url->toString();

    // Redirect to the WebGL application page with the query string.
    return new RedirectResponse($destination);
  }

  public function itinerari()
  {
    return array(
      // '#type' => 'markup',
      '#theme' => 'phygital_itinerari',
      '#attached' => [
        'library' => [
          'phygital/leafletlib',
          'phygital/phygital_itinerari',
        ],
      ],
    );
  }
  public function itinerario()
  {
    $language = \Drupal::languageManager()->getCurrentLanguage()->getId();
    return array(
      // '#type' => 'markup',
      '#theme' => 'phygital_itinerario',
      '#lng' => $language,
      '#attached' => [
        'library' => [
          'phygital/phygital_itinerario',
        ],
      ],
    );
  }
  public function benvenuto()
  {
    return array(
      // '#type' => 'markup',
      '#theme' => 'phygital_benvenuto',
      '#attached' => [
        'library' => [
          'phygital/phygital_benvenuto',
        ],
      ],
    );
  }
  public function creaitinerario()
  {
    $language = \Drupal::languageManager()->getCurrentLanguage()->getId();
    return array(
      // '#type' => 'markup',
      '#theme' => 'phygital_creaitinerario',
      '#lng' => $language,
      '#attached' => [
        'library' => [
          'phygital/phygital_creaitinerario',
        ],
      ],
    );
  }
  public function creaitinerario_comune()
  {
    return array(
      // '#type' => 'markup',
      '#theme' => 'phygital_creaitinerario_comune',
      '#attached' => [
        'library' => [
          'phygital/phygital_creaitinerario_comune',
        ],
      ],
    );
  }
}
