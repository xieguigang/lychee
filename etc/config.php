<?php

# rename this file to config.php to run this website

// 网站的配置数据
return [
    'DB_TYPE' => 'mysql',
    'DB_HOST' => '127.0.0.1',
    'DB_NAME' => 'yilia',
    'DB_USER' => 'root',
    'DB_PWD'  => '123456',
    'DB_PORT' => '3306',
    
    // key to decode user password 
    // modify this config value will make all existed user 
    // password invalid in database
    "AUTHCODE" => 'A2f0qS78ttR9HpqeOT',
    //cookies
    "COOKIE_PREFIX"       => '8FSUix_',
    "ERR_HANDLER_DISABLE" => "FALSE",

    // 自定义http错误页面的位置，例如404 500 403等
    "RFC7231"       => APP_PATH . "/views/http_errors/",
    "CACHE"         => true,
    "TEMP"          => "Z:/php/",
    "APP_NAME"      => "yilia",
	"APP_VERSION"   => "0.222.58-alpha",
    "MVC_VIEW_ROOT" => [
        "passport"       => APP_PATH . "/views/user/",
        "app"            => APP_PATH . "/views/Application/",
        "index"          => APP_PATH . "/views/",
        "pathway_design" => APP_PATH . "/views/Application/pathway_designer/",
        "admin"          => APP_PATH . "/views/admin/",
        "task"           => APP_PATH . "/views/Application/task"
    ]
];
