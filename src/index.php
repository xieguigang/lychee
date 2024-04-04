<?php

include __DIR__ . "/../etc/bootstrap.php";

class App {

    /**
     * @access *
     * @uses view
    */
    public function index() {
        View::Display();
    }

    /**
     * @access *
     * @uses view
    */
    public function album($id) {
        View::Display();
    }
}