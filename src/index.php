<?php

include __DIR__ . "/../etc/bootstrap.php";

class App {

    /**
     * Lychee
     * 
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

    /**
     * Gallery
     * 
     * @access *
     * @uses view
    */
    public function gallery() {
        View::Display();
    }
}