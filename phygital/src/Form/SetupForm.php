<?php

namespace Drupal\phygital\Form;

use Drupal\Core\Form\ConfigFormBase;
use Drupal\Core\Form\FormStateInterface;
use Drupal\Core\Form\FormBase;
use Drupal\file\Entity\File;
use Drupal\node\Entity\Node;
use Drupal\Core\File\FileSystemInterface;
use Drupal\Core\Messenger\MessengerInterface;
use Drupal\Core\Config\ConfigFactoryInterface;
use Drupal\Core\Routing\RouteMatchInterface;
use Symfony\Component\DependencyInjection\ContainerInterface;
use Symfony\Component\HttpFoundation\JsonResponse;
use Drupal\Core\Url;
use Drupal\Core\Directory\FileSystemInterface as DirectoryFileSystemInterface;
use Drupal\file\FileInterface;
use Drupal\Core\StreamWrapper\StreamWrapperManagerInterface;



class SetupForm extends ConfigFormBase {


  protected $fileSystem;
  /**
   * {@inheritdoc}
   */


  /**/

  protected $messenger;

  public function __construct(ConfigFactoryInterface $config_factory, FileSystemInterface $file_system, MessengerInterface $messenger) {
    parent::__construct($config_factory);
    $this->fileSystem = $file_system;
    $this->messenger = $messenger;
  }

  public static function create(ContainerInterface $container) {
    return new static(
      $container->get('config.factory'),
      $container->get('file_system'),
      $container->get('messenger')
    );
  }
  /**/


  public function getFormId()
  {
    return 'phygital_csv_json_form';
  }

  /**
   * {@inheritdoc}
   */
  public function buildForm(array $form, FormStateInterface $form_state){
    //$config = $this->config('csvjson.settings');
    // Recupera l'URL del file JSON dalla configurazione.
    $config = $this->config('csvjson.settings');
    $jsonFileUrl = $config->get('json_file_url');
    $jsonFileUrlLt = $config->get('json_file_urlLt');

    $form = parent::buildForm($form, $form_state);

    $allowed_ext = 'csv';
    $max_upload = 25600000;

    $last_execution_date=$config->get('last_running_date');
    $file_csv=$config->get('files_csv');
    $file_blk=$config->get('files_blk');
    $form['text_header'] = [
      '#prefix' => '<p>',
      '#suffix' => '</p>',
      '#markup' => t('Gestione conversione Sferiche csv a json<br>Nel caso in cui viene salvato il form senza associare un csv il file json viene azzerato'),
      '#weight' => -100,
    ];

    $form['message'] = [
      '#type' => 'markup',
      '#markup' => '<div id="result-message"></div>'
    ];
    // Aggiungi il campo "versione".
    $form['versione'] = [
      '#type' => 'textfield',
      '#title' => $this->t('Versione'),
      '#required' => TRUE, // Imposta il campo come obbligatorio.
      '#default_value' => $config->get('versione'), // Imposta il valore predefinito dal valore nel file di configurazione.
    ];
    $form['files_csv'] = [
      '#type' => 'managed_file',
      '#title' => $this->t('Sferiche CSV'),
      '#required' => TRUE, // Rendi il caricamento del file CSV obbligatorio.
      '#multiple' => FALSE,
      '#description' => $this->t('File consentiti: @allowed_ext', ['@allowed_ext' => $allowed_ext]),
      '#upload_location' => 'public://sferiche/',
      '#default_value' => $file_csv,
      '#upload_validators' => [
        'file_validate_extensions' => [
          $allowed_ext,
        ],
        'file_validate_size' => [
          $max_upload,
        ],
      ],
    ];

    $form['files_blk'] = [
      '#type' => 'managed_file',
      '#title' => $this->t('Blacklist CSV'),
      '#required' => TRUE, // Rendi il caricamento del file CSV obbligatorio.
      '#multiple' => FALSE,
      '#description' => $this->t('File consentiti: @allowed_ext', ['@allowed_ext' => $allowed_ext]),
      '#upload_location' => 'public://sferiche/',
      '#default_value' => $file_blk,
      '#upload_validators' => [
        'file_validate_extensions' => [
          $allowed_ext,
        ],
        'file_validate_size' => [
          $max_upload,
        ],
      ],
    ];



    // Aggiungi il link al file JSON nel form.
    if (!empty($jsonFileUrl)) {
      $form['json_file_link'] = [
        '#type' => 'link',
        '#title' => $this->t('JSON completo'),
        '#url' => Url::fromUri($jsonFileUrl),
        '#attributes' => array(
          'class' => array('button'),
          'target' => '_blank',
        ),
      ];
    }
    if (!empty($jsonFileUrlLt)) {
      $form['json_file_linkLt'] = [
        '#type' => 'link',
        '#title' => $this->t('JSON lite'),
        '#url' => Url::fromUri($jsonFileUrlLt),
        '#attributes' => array(
          'class' => array('button'),
          'target' => '_blank',
        ), // Apre il link in una nuova finestra.
      ];
    }
    return $form;
  }

  public function submitForm(array &$form, FormStateInterface $form_state) {
    // Ottieni il valore del campo "versione" dal form.
    $versione = $form_state->getValue('versione');

    // Verifica se il campo "files_csv" è stato modificato o se sono stati caricati nuovi file.
    $files_uploaded = false;
    $files_csv_old = $this->configFactory->get('csvjson.settings')->get('files_csv') ?? [];
    $files_csv_new = $form_state->getValue('files_csv');
    //$files_uploaded = !empty(array_diff($files_csv_new, $files_csv_old));
    $files_uploaded= (empty($files_csv_old)||(!empty(array_diff($files_csv_new, $files_csv_old)))) ? true:false;

    // Verifica se il campo "files_csv" è stato modificato o se sono stati caricati nuovi file.
    $files_uploaded_blk = false;
    $files_blk_old = $this->configFactory->get('csvjson.settings')->get('files_blk') ?? [];
    $files_blk_new = $form_state->getValue('files_blk');
    //$files_uploaded_blk = !empty(array_diff($files_csv_new, $files_csv_old));
      $files_uploaded_blk= (empty($files_blk_old)||(!empty(array_diff($files_blk_new, $files_blk_old)))) ? true:false;

$xx="";
    // Verifica se il campo "versione" è stato modificato.
    $versione_old = $this->configFactory->get('csvjson.settings')->get('versione') ?? '';
    $versione_changed = $versione_old !== $versione;

    if ($files_uploaded || $versione_changed || $files_uploaded_blk ) {
      // Se sono stati caricati nuovi file o è stata cambiata la versione, procedi con il salvataggio dei dati e la generazione del JSON.
      $fid = reset($form_state->getValue('files_csv'));
      $fid_blk = reset($form_state->getValue('files_blk'));
      $file = File::load($fid);
      $file_blk = File::load($fid_blk);

      if ($file && $file_blk) {
        $file->setPermanent();
        $file_blk->setPermanent();
        $file->save();
        $file_blk->save();
        $file_path = $file->getFileUri();
        $file_path_blk = $file_blk->getFileUri();
        $jsonData = $this->csvToJson($file_path,$file_path_blk);
        $xx="";
      } else {
        // Display an error message if there was an issue loading the uploaded file.
        $this->messenger->addError($this->t('Errore durante il caricamento del file CSV.'));
        return;
      }

      $data = [
        'versione' => $versione,
        'Base_url' => '',
        'sferiche' => $jsonData[0],
      ];
      $dataLt = [
        'versione' => $versione,
        'Base_url' => '',
        'sferiche' => $jsonData[1],
      ];

      // Save the JSON data to the configuration.
      $config = $this->configFactory->getEditable('csvjson.settings');
      $config->set('versione', $versione);
      $config->set('files_csv', $form_state->getValue('files_csv'));
      $config->set('files_blk', $form_state->getValue('files_blk'));
      $config->set('json_data', $data);
      $config->set('json_data_lt', $dataLt);
      $config->save();

      $jsonFilePath = 'public://sferiche/sferiche.json';
      $jsonString = json_encode($data, JSON_PRETTY_PRINT);
      $jsonFilePathLt = 'public://sferiche/sfericheLt.json';
      $jsonStringLt = json_encode($dataLt, JSON_PRETTY_PRINT);

      // Salva il JSON nella cartella pubblica.
      $temporaryFilePath = 'temporary://sferiche.json';
      file_save_data($jsonString, $temporaryFilePath, FileSystemInterface::EXISTS_REPLACE);
      $temporaryFilePathLt = 'temporary://sfericheLt.json';
      file_save_data($jsonStringLt, $temporaryFilePathLt, FileSystemInterface::EXISTS_REPLACE);

      // Ottieni il servizio file_system.
      $fileSystem = \Drupal::service('file_system');

      // Sposta il file temporaneo nella cartella pubblica.
      $fileSystem->move($temporaryFilePath, $jsonFilePath, FileSystemInterface::EXISTS_REPLACE);
      $fileSystem->move($temporaryFilePathLt, $jsonFilePathLt, FileSystemInterface::EXISTS_REPLACE);

      // Verifica se il file JSON è stato salvato correttamente.
      if ($fileSystem->realpath($jsonFilePath)) {
        // Imposta l'URL del file JSON nella configurazione.
        $config->set('json_file_url', file_create_url($jsonFilePath));
        $config->save();

        // Display a success message to the user.
        $this->messenger->addStatus($this->t('Il file CSV è stato convertito in JSON e memorizzato in configurazione.'));
        $this->messenger->addStatus($this->t('La Versione attuale è: @versione', ['@versione' => $versione]));
      } else {
        // Display an error message if there was an issue saving the JSON file.
        $this->messenger->addError($this->t('Errore durante il salvataggio del file JSON nella cartella pubblica.'));
      }
      // Verifica se il file JSON è stato salvato correttamente.
      if ($fileSystem->realpath($jsonFilePathLt)) {
        // Imposta l'URL del file JSON nella configurazione.
        $config->set('json_file_urlLt', file_create_url($jsonFilePathLt));
        $config->save();

        // Display a success message to the user.
        $this->messenger->addStatus($this->t('Il file CSV è stato convertito in JSON e memorizzato in configurazione.'));
        $this->messenger->addStatus($this->t('La Versione attuale è: @versione', ['@versione' => $versione]));
      } else {
        // Display an error message if there was an issue saving the JSON file.
        $this->messenger->addError($this->t('Errore durante il salvataggio del file JSON nella cartella pubblica.'));
      }
      $this->generateVersioneSfericheJson($versione);
    } else {
      // Se non sono stati caricati nuovi file o modificata la "versione",
      // mostra un messaggio di avviso.
      $this->messenger->addWarning($this->t('Nessun nuovo file CSV o modifiche alla versione. La cartella sferiche verrà azzerata solo se hai caricato nuovi file CSV.'));

      // Elimina solo i file caricati nella cartella "sferiche".
      $sferiche_directory = 'public://sferiche/';
      foreach ($files_csv_old as $file_id) {
        $file = File::load($file_id);
        if ($file && strpos($file->getFileUri(), $sferiche_directory) === 0) {
          $file->delete();
        }
      }

      // Reimposta solo il campo "files_csv".
      $config = $this->configFactory->getEditable('csvjson.settings');
      $config->set('files_csv', []);
      $config->save();

      // Imposta solo il campo "versione" nel form a vuoto.
      $form_state->setValue('versione', '');
    }

    return parent::submitForm($form, $form_state);
  }


  public function validateForm(array &$form, FormStateInterface $form_state) {
    $files_csv = $form_state->getValue('files_csv');

    // Verifica se il campo del file CSV è vuoto.
    if (empty($files_csv)) {
      $form_state->setErrorByName('files_csv', $this->t('Il campo del file CSV è obbligatorio. Seleziona un file CSV da caricare.'));
    }
  }

  /**
   * {@inheritdoc}
   */
  protected function getEditableConfigNames() {
    return [
      'csvjson.settings',
    ];
  }

  /**
   * Genera il file JSON "versionesferiche.json" con la versione.
   *
   * @param string $versione
   *   La versione da salvare nel file JSON.
   */
  private function generateVersioneSfericheJson($versione) {
    // Crea l'array con i dati per il file JSON.
    $data = [
      'versione' => $versione,
    ];

    // Converti l'array in JSON.
    $jsonString = json_encode($data, JSON_PRETTY_PRINT);

    // Salva il JSON nella cartella pubblica.
    $jsonFilePath = 'public://sferiche/versionesferiche.json';
    file_put_contents($jsonFilePath, $jsonString);
  }

  private function csvToJson($csvFilePath,$blkFilePath) {
    //carico il file delle blacklist
    $csvData = [];
    if (($handle_blk = fopen($blkFilePath, 'r')) !== FALSE) {
      while (($data = fgetcsv($handle_blk)) !== false) {
        // Assuming your CSV has only one column, and the data is in the first column.
        $csvData[] = $data[0];
      }
      fclose($handle_blk);
    }

    // Initialize an empty array to store the JSON data.
    $jsonData = array();
    $jsonDataLite = array();
    $progressiveId = 1;
    $progressiveIdLt= 1;
    // Open the CSV file for reading.
    if (($handle = fopen($csvFilePath, 'r')) !== FALSE) {
      // Read the header row to get the column names.
      $header = fgetcsv($handle, 0, "\t");
      //$header_lite = fgetcsv($handle, 0, "\t");

      // Process each row in the CSV file.
      while (($data = fgetcsv($handle, 0, "\t")) !== FALSE) {
        $row = array();
        $row_lite = array();
        $row['Id'] = $progressiveId;
        $row_lite['Id'] = $progressiveIdLt;
        $progressiveId++;
        $progressiveIdLt++;
        $excludeRow = false; // Flag to indicate whether to exclude the row
        // Loop through the columns in the header and add the values to the row array.
        foreach ($header as $index => $columnName) {
          // Keep only the desired columns ("X", "Y", and "Filename").
		  //if (in_array($columnName, ['X', 'Y', 'Timestamp', 'Direction_', 'Northing', 'Height_', 'Up_Easting', 'Roll_X_deg', 'Pitch_Y_de', 'Yaw_Z_deg_', 'Omega_deg_', 'Phi_deg_', 'Kappa_deg_', 'ELEVATION', 'Filename'])) {
          if (in_array($columnName, ['X', 'Y', 'Omega_deg_', 'Phi_deg_', 'Kappa_deg_', 'Filename'])) {
            // Remove quotes from the elements and replace commas with dots for numeric values.
            $value = str_replace('"', '', $data[$index]);
            $value = str_replace(',', '.', $value);
            $row[$columnName] = $value;
            // Check if the 'Filename' value is present in $csvData.
            if ($columnName === 'Filename' && in_array($value, $csvData)) {
              $excludeRow = true; // Set the flag to exclude this row
              break; // No need to continue checking, we can exit the loop
            }
          }
        }
        if (!$excludeRow) {
          foreach ($header as $index => $columnName) {
            // Keep only the desired columns ("X", "Y").
            if (in_array($columnName, ['X', 'Y','Filename'])) {
              // Remove quotes from the elements and replace commas with dots for numeric values.
              $value = str_replace('"', '', $data[$index]);
              $value = str_replace(',', '.', $value);
              $row_lite[$columnName] = $value;
            }
          }

          // Add the row to the JSON data arrays only if it's not excluded.
          $jsonData[] = $row;
          $jsonDataLite[] = $row_lite;
        }
      }

      // Close the CSV file.
      fclose($handle);
    }
    $result=array();
    $result=[$jsonData,$jsonDataLite];
   //return $jsonData;
    return $result;
  }

}
