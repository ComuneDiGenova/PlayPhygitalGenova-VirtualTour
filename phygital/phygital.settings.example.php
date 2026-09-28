<?php

/**
 * @file
 * Template for phygital.settings.php.
 *
 * Copy this file to phygital.settings.php (same folder) and fill in the values.
 * phygital.settings.php is ignored by git.
 */

return [
  // visitgenoa JSON API, used by fetch_api_call.php.
  'api' => [
    'base_url' => 'https://www.visitgenoa.it/jsonapi/',
    'username' => '',
    'password' => '',
  ],
  // OpenRouteService, used by ors_proxy.php.
  'ors' => [
    'api_key' => '',
    'url' => 'https://api.openrouteservice.org/v2/directions/foot-walking',
  ],
  // Database holding the ors_cache table, used by ors_proxy.php.
  'db' => [
    'host' => 'localhost',
    'database' => '',
    'user' => '',
    'password' => '',
  ],
  // Public values exposed to the JS as drupalSettings.phygital.
  'js' => [
    'dev_root' => 'https://dev.phygital.bbsitalia.com',
    'prod_root' => 'https://www.visitgenoa.it',
    // User forced when browsing from dev_root.
    'dev_user_code' => '',
    'dev_user_id' => 0,
  ],
];
