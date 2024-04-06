<?php

include __DIR__ . "/../etc/bootstrap.php";

class App {

    /**
     * Just a single user in current platform, for private used only, 
     * not design for multiple user
     * 
     * @uses api
     * @access *
     * @method POST
    */
    public function login($email, $passwd) {
        $settings = new Table("settings");
        $config = $settings->where(["name" => "user"])->find();
        $passwd = md5("lychee" . $passwd);

        if (Strings::Empty($email) || Strings::Empty($passwd)) {
            controller::error("login email or password could not be empty!");
        }

        if (Utils::isDbNull($config)) {
            # access directly
            # and create new account
            $config = ["email" => $email, "passwd" => $passwd];
            $settings->add([
                "name" => "user",
                "configs" => json_encode($config)
            ]);
        } else {
            $config = json_decode($config["configs"], true);
        }

        if ($passwd != $config["passwd"] || strtolower($email) != strtolower($config["email"])) {
            controller::error("login email or password error!");
        } else {
            controller::success(1);
        }
    }
}