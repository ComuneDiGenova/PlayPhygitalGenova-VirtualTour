<?php

header("Access-Control-Allow-Origin: *");

$username = '16892599758';
$password = 'mas7Hsg5$q?a';

$base_url = 'https://testnew.visitgenoa.it/jsonapi/';

if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    // Fetch the data from the incoming POST request
    $endpoint = $_POST['endpoint'];
    $url = $base_url . $endpoint;

    // Set the options for cURL
    $options = array(
        CURLOPT_URL => $url,
        CURLOPT_RETURNTRANSFER => true,
        CURLOPT_POST => true,
        CURLOPT_POSTFIELDS => $_POST,
        CURLOPT_SSL_VERIFYPEER => false,
        CURLOPT_SSL_VERIFYHOST => false
    );

    // Initialize cURL session
    $ch = curl_init();
    curl_setopt_array($ch, $options);
    curl_setopt($ch, CURLOPT_HTTPAUTH, CURLAUTH_BASIC);
    curl_setopt($ch, CURLOPT_USERPWD, "$username:$password");

    // Execute cURL and get the response
    $response = curl_exec($ch);

    // Check for cURL errors
    if (curl_errno($ch)) {
        echo 'Error: ' . curl_error($ch);
    }

    // Close cURL session
    curl_close($ch);

    // Return the response as plain text
    header('Content-Type: text/plain');
    echo $response;
} elseif ($_SERVER['REQUEST_METHOD'] === 'GET') {

    $endpoint = $_GET['endpoint'];
    $url = $base_url . $endpoint;
    $username = '16892599758';
    $password = 'mas7Hsg5$q?a';

    $ch = curl_init($url);
    curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
    curl_setopt($ch, CURLOPT_SSL_VERIFYPEER, false);
    curl_setopt($ch, CURLOPT_SSL_VERIFYHOST, false);
    curl_setopt($ch, CURLOPT_HTTPHEADER, [
        'Authorization: Basic ' . base64_encode($username . ':' . $password)
    ]);

    // Disable automatic redirect handling
    curl_setopt($ch, CURLOPT_FOLLOWLOCATION, false);

    $response = curl_exec($ch);

    if (curl_errno($ch)) {
        echo 'cURL error: ' . curl_error($ch);
    }

    // Check if it's a redirect response
    $httpCode = curl_getinfo($ch, CURLINFO_HTTP_CODE);
    if ($httpCode >= 300 && $httpCode < 400) {
        $redirectUrl = curl_getinfo($ch, CURLINFO_REDIRECT_URL);
        // Make a new request to the redirected URL
        curl_setopt($ch, CURLOPT_URL, $redirectUrl);
        $response = curl_exec($ch);
    }

    curl_close($ch);

    echo $response;
} elseif ($_SERVER['REQUEST_METHOD'] === 'PUT') {
    // Fetch the data from the incoming PUT request
    parse_str(file_get_contents("php://input"), $_PUT);
    $endpoint = $_PUT['endpoint'];
    $url = $base_url . $endpoint;

    // Set the options for cURL (similar to POST, but with PUT method and PUT data)
    $options = array(
        CURLOPT_URL => $url,
        CURLOPT_RETURNTRANSFER => true,
        CURLOPT_CUSTOMREQUEST => 'PUT', // Use PUT method
        CURLOPT_POSTFIELDS => http_build_query($_PUT), // PUT data
        CURLOPT_SSL_VERIFYPEER => false,
        CURLOPT_SSL_VERIFYHOST => false
    );

    // Initialize cURL session
    $ch = curl_init();
    curl_setopt_array($ch, $options);
    curl_setopt($ch, CURLOPT_HTTPAUTH, CURLAUTH_BASIC);
    curl_setopt($ch, CURLOPT_USERPWD, "$username:$password");

    // Execute cURL and get the response
    $response = curl_exec($ch);

    // Check for cURL errors
    if (curl_errno($ch)) {
        echo 'Error: ' . curl_error($ch);
    }

    // Close cURL session
    curl_close($ch);

    // Return the response as plain text
    header('Content-Type: text/plain');
    echo $response;
} elseif ($_SERVER['REQUEST_METHOD'] === 'DELETE') {
    // Fetch the data from the incoming DELETE request
    parse_str(file_get_contents("php://input"), $_DELETE);
    $endpoint = $_DELETE['endpoint'];
    $url = $base_url . $endpoint;

    // Set the options for cURL (similar to POST, but with DELETE method and DELETE data)
    $options = array(
        CURLOPT_URL => $url,
        CURLOPT_RETURNTRANSFER => true,
        CURLOPT_CUSTOMREQUEST => 'DELETE', // Use DELETE method
        CURLOPT_POSTFIELDS => http_build_query($_DELETE), // DELETE data (if needed)
        CURLOPT_SSL_VERIFYPEER => false,
        CURLOPT_SSL_VERIFYHOST => false
    );

    // Initialize cURL session
    $ch = curl_init();
    curl_setopt_array($ch, $options);
    curl_setopt($ch, CURLOPT_HTTPAUTH, CURLAUTH_BASIC);
    curl_setopt($ch, CURLOPT_USERPWD, "$username:$password");

    // Execute cURL and get the response
    $response = curl_exec($ch);

    // Check for cURL errors
    if (curl_errno($ch)) {
        echo 'Error: ' . curl_error($ch);
    }

    // Close cURL session
    curl_close($ch);

    // Return the response as plain text
    header('Content-Type: text/plain');
    echo $response;
} else {
    echo 'Error: Invalid request method. Only POST, PUT, DELETE, and GET requests are allowed.';
}
