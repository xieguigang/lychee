<?php

imports("MVC.controller");
imports("RFC7231.logger");
imports("RFC7231.index");

/**
 * 用户访问权限控制器
*/
class accessController extends controller {

    function __construct() {
        parent::__construct();
    }

    public function accessControl() {       
        if ($this->AccessByEveryOne()) {
            return true;
        }

        if (!empty($_SESSION)) {
            if (array_key_exists("user", $_SESSION)) {
                return true;
            }
        }

        return false;
    }

    /**
     * 假若没有权限的话，会执行这个函数进行重定向
    */
    public function Redirect($code) {
        $url = urlencode(Utils::URL());
        $url = "/login?goto=$url";

        \Redirect($url);
    }   
}