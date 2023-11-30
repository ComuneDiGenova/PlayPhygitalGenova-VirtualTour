<?php

header("Access-Control-Allow-Origin: *");

// Make sure this script is accessed using the GET method only
if ($_SERVER['REQUEST_METHOD'] !== 'GET') {
    http_response_code(405); // Method Not Allowed
    die('Method not allowed.');
}

// Get the URL to proxy from the "url" query parameter in the GET request
$url = $_GET['url'] ?? '';
$url = str_replace("%", "%25", $url);
$url = str_replace(" ", "%20", $url);
// Validate the URL to prevent potential security risks
// if (filter_var($url, FILTER_VALIDATE_URL) === false) {
//     http_response_code(400); // Bad Request
//     die('Invalid URL');
// }

//file_put_contents("log.txt",$url);
// $url = "https://phygitalfoto360.s3.eu-south-1.amazonaws.com/ridimensionate-alte/Job%20014-%20Setup%20049.jpg";

// Initialize a cURL session
$ch = curl_init();

// Set the cURL options
curl_setopt($ch, CURLOPT_URL, $url);
curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
curl_setopt($ch, CURLOPT_SSL_VERIFYPEER, false); // Disable certificate verification
curl_setopt($ch, CURLOPT_SSL_VERIFYHOST, false); // Disable host name verification

// Execute the cURL session and get the response
$response = curl_exec($ch);

// Check for cURL errors
if (curl_errno($ch)) {
    http_response_code(500); // Internal Server Error
    // file_put_contents("log.txt",'Curl Error: ' . curl_error($ch));
    die('Curl Error: ' . curl_error($ch));
}

// Get the HTTP status code of the response
$httpCode = curl_getinfo($ch, CURLINFO_HTTP_CODE);

// Close the cURL session
curl_close($ch);

// Set the appropriate Content-Type header based on the response
$mime_type = curl_getinfo($ch, CURLINFO_CONTENT_TYPE);
header('Content-Type: ' . $mime_type);

// Set the Content-Disposition header to force download
$filename = basename($url);
header('Content-Disposition: attachment; filename="' . $filename . '"');

// Set the HTTP status code of the proxy response based on the original response
http_response_code($httpCode);

// Output the response from the target URL
echo $response;
