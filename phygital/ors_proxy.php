<?php
// Settings live in phygital.settings.php, next to this file or in the module folder (when this file is copied into /web)
$settings = require (file_exists(__DIR__ . '/phygital.settings.php') ? __DIR__ : __DIR__ . '/modules/custom/phygital') . '/phygital.settings.php';

//https://openrouteservice.org/dev/#/home
$apiKey = $settings['ors']['api_key'];

// The URL to the OpenRouteService API
$openRouteServiceUrl = $settings['ors']['url'];

// Get the query parameters from the client-side request
$queryParams = http_build_query($_GET);

$host = $settings['db']['host'];
$database = $settings['db']['database'];
$db_user = $settings['db']['user'];
$db_pass = $settings['db']['password'];

$con = new mysqli($host, $db_user, $db_pass, $database); 
if ($con->connect_error) { die("Connection failed: " . $con->connect_error); }

if (!$result = mysqli_query($con, "SELECT `value` FROM ors_cache WHERE `key` = '$queryParams'")) { print("<br><br>ERRORI SQL:<br>"); die(mysqli_error($con)); }
if (mysqli_num_rows($result)>0) {
    $row = mysqli_fetch_array($result);
    $response_escaped = $row[0];
    $response = str_replace(array('\\\\', '\\0', '\\n', '\\r', "\\'", '\\"', '\\Z'), array('\\', "\0", "\n", "\r", "'", '"', "\x1a"), $response_escaped);
}
else {
    // Create the final URL for the OpenRouteService API request
    $finalUrl = "$openRouteServiceUrl?$queryParams&api_key=$apiKey";

    // Forward the request to the OpenRouteService API and get the response
    if (($response = file_get_contents($finalUrl)) === false) {
        $error = error_get_last();
        echo "HTTP request failed. Error was: " . $error['message'];
    } else {
        $response_escaped = str_replace(array('\\', "\0", "\n", "\r", "'", '"', "\x1a"), array('\\\\', '\\0', '\\n', '\\r', "\\'", '\\"', '\\Z'), $response);
        try {
            if (mysqli_query($con, "INSERT INTO ors_cache VALUES ('$queryParams','$response_escaped')") === TRUE) {
            } else {
              echo "Error: " . $sql . "<br>" . $conn->error;
            }
        }
        catch (Exception $e) {            
        }
    }
}

$con->close();


// Set the appropriate headers to allow cross-origin requests
header('Content-Type: application/json');
header('Access-Control-Allow-Origin: *');

// Send the response back to the client-side JavaScript
echo $response;