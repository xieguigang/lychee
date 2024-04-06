<?php

class session {

    public static function write_user_session($email) {
        $_SESSION["user"] = $email;
    }
}