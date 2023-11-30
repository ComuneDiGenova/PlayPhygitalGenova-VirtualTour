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



class AppForm extends ConfigFormBase {


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
    return 'phygital_app_form';
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

    $form['text_header'] = [
      '#prefix' => '<p>',
      '#suffix' => '</p>',
      '#markup' => t('Gestione versionamento, AppStore e PlayStore'),
      '#weight' => -100,
    ];

    $form['message'] = [
      '#type' => 'markup',
      '#markup' => '<div id="result-message"></div>'
    ];
    // Aggiungi il campo "versione".
    $form['versione_app'] = [
      '#type' => 'textfield',
      '#title' => $this->t('Versione'),
      '#required' => TRUE, // Imposta il campo come obbligatorio.
      '#default_value' => $config->get('versione_app'), // Imposta il valore predefinito dal valore nel file di configurazione.
    ];

	$form['appstore_app'] = [
      '#type' => 'textfield',
      '#title' => $this->t('AppStore'),
      '#required' => TRUE, // Imposta il campo come obbligatorio.
      '#default_value' => $config->get('appstore_app'), // Imposta il valore predefinito dal valore nel file di configurazione.
    ];

    $form['playstore_app'] = [
      '#type' => 'textfield',
      '#title' => $this->t('PlayStore'),
      '#required' => TRUE, // Imposta il campo come obbligatorio.
      '#default_value' => $config->get('playstore_app'), // Imposta il valore predefinito dal valore nel file di configurazione.
    ];


    return $form;
  }

  public function submitForm(array &$form, FormStateInterface $form_state) {
    // Ottieni il valore del campo "versione" dal form.
    $versione = $form_state->getValue('versione_app');
    $appstore = $form_state->getValue('appstore_app');
	  $playstore = $form_state->getValue('playstore_app');
    $this->generateAppSettingsJson($versione,$appstore,$playstore);
    $config = $this->configFactory->getEditable('csvjson.settings');
    $config->set('versione_app', $versione);
    $config->set('appstore_app', $appstore);
    $config->set('playstore_app', $playstore);
    $config->save();
$xx="";




    return parent::submitForm($form, $form_state);
  }


  public function validateForm(array &$form, FormStateInterface $form_state) {
    $versione = $form_state->getValue('versione_app');
    $appstore = $form_state->getValue('appstore_app');
    $playstore = $form_state->getValue('playstore_app');

    // Verifica se il campo del file CSV è vuoto.
    if (empty($versione)) {
      $form_state->setErrorByName('Versione', $this->t('Il campo è obbligatorio.'));
    }
    if (empty($appstore)) {
      $form_state->setErrorByName('AppStore', $this->t('Il campo è obbligatorio.'));
    }
    if (empty($playstore)) {
      $form_state->setErrorByName('PlayStore', $this->t('Il campo è obbligatorio.'));
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
  private function generateAppSettingsJson($versione,$appstore,$playstore) {
    // Crea l'array con i dati per il file JSON.
    $data = [
      'versione' => $versione,
      'appstore' => $appstore,
      'playstore' => $playstore,
    ];

    // Converti l'array in JSON.
    $jsonString = json_encode($data, JSON_PRETTY_PRINT);

    // Salva il JSON nella cartella pubblica.
    $jsonFilePath = 'public://sferiche/versione_app.json';
    file_put_contents($jsonFilePath, $jsonString);
  }


}
