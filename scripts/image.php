<?php

class image {

    public static function process_upload($raw) {
        include_once APP_PATH . "/framework/PHPExif/lib/PHPExif/Reader/Reader.php";
breakpoint($raw);
        // reader with Native adapter
        $reader = \PHPExif\Reader\Reader::factory(\PHPExif\Enum\ReaderType::NATIVE);
        $exif = $reader->read($raw);

        breakpoint($exif);
    }
}