<?php

include_once APP_PATH . "/framework/Safe/lib/DateTime.php";

include_once APP_PATH . "/framework/Safe/generated/fileinfo.php";
include_once APP_PATH . "/framework/Safe/generated/filesystem.php";
include_once APP_PATH . "/framework/Safe/generated/image.php";
include_once APP_PATH . "/framework/Safe/generated/pcre.php";
include_once APP_PATH . "/framework/Safe/generated/datetime.php";

include_once APP_PATH . "/framework/Utf8/ForceUTF8/Encoding.php";

include_once APP_PATH . "/framework/PHPExif/lib/PHPExif/Contracts/AdapterInterface.php";
include_once APP_PATH . "/framework/PHPExif/lib/PHPExif/Contracts/HydratorInterface.php";
include_once APP_PATH . "/framework/PHPExif/lib/PHPExif/Contracts/MapperInterface.php";
include_once APP_PATH . "/framework/PHPExif/lib/PHPExif/Contracts/ReaderInterface.php";
include_once APP_PATH . "/framework/PHPExif/lib/PHPExif/Enum/ReaderType.php";
include_once APP_PATH . "/framework/PHPExif/lib/PHPExif/Exif.php";

include_once APP_PATH . "/framework/PHPExif/lib/PHPExif/Adapter/AbstractAdapter.php";
include_once APP_PATH . "/framework/PHPExif/lib/PHPExif/Adapter/Exiftool.php";
include_once APP_PATH . "/framework/PHPExif/lib/PHPExif/Adapter/FFprobe.php";
include_once APP_PATH . "/framework/PHPExif/lib/PHPExif/Adapter/ImageMagick.php";
include_once APP_PATH . "/framework/PHPExif/lib/PHPExif/Adapter/Native.php";
include_once APP_PATH . "/framework/PHPExif/lib/PHPExif/Hydrator/Mutator.php";
include_once APP_PATH . "/framework/PHPExif/lib/PHPExif/Mapper/AbstractMapper.php";
include_once APP_PATH . "/framework/PHPExif/lib/PHPExif/Mapper/Exiftool.php";
include_once APP_PATH . "/framework/PHPExif/lib/PHPExif/Mapper/FFprobe.php";
include_once APP_PATH . "/framework/PHPExif/lib/PHPExif/Mapper/ImageMagick.php";
include_once APP_PATH . "/framework/PHPExif/lib/PHPExif/Mapper/Native.php";
include_once APP_PATH . "/framework/PHPExif/lib/PHPExif/Reader/PhpExifReaderException.php";
include_once APP_PATH . "/framework/PHPExif/lib/PHPExif/Reader/Reader.php";

class image {

    public static function process_upload($raw) {
        include_once APP_PATH . "/framework/PHPExif/lib/PHPExif/Reader/Reader.php";

        // reader with Native adapter
        $reader = \PHPExif\Reader\Reader::factory(\PHPExif\Enum\ReaderType::NATIVE);
        $exif = $reader->read($raw);

        breakpoint($exif);
    }
}