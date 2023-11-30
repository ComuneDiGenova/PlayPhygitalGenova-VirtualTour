<?php
// Get the API key from the client-side request (modify this to suit your actual implementation)
//https://openrouteservice.org/dev/#/home
//ACCOUNT davidebonomo pwd Bettolino@3 mail davide.bonomo@bbsitalia.com
$apiKey = '5b3ce3597851110001cf62483450b975f6114d298b17997b2dbd0ee9';

// The URL to the OpenRouteService API
$openRouteServiceUrl = 'https://api.openrouteservice.org/v2/directions/foot-walking';

// Get the query parameters from the client-side request
$queryParams = http_build_query($_GET);

$host = 'localhost';
$database = 'dev_phygital';
$db_user = 'usr_phygital';
$db_pass = 'P5t6%eri$23!';

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