<?php

define("APP_PATH", dirname(__DIR__));
define("APP_UPLOAD", APP_PATH . "/data/raw");
define("IMAGE_PUBLIC", APP_PATH . "/data/large");
define("IMAGE_THUMBS", APP_PATH . "/data/thumbs");
define("APP_DEBUG", false);
define("YEAR", date("Y"));

define("slogan", "Capture moments, share memories, let life's moments flow in our memories.");

session_start();

include APP_PATH . "/framework/php.NET/package.php";
include APP_PATH . "/etc/access.php";

dotnet::AutoLoad(APP_PATH . "/etc/config.php");
dotnet::HandleRequest(new App(), new accessController());