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

include_once APP_PATH . "/framework/SimpleImage.php";

class image {

    public static function process_upload($raw, $upload_raw, $name, $size, $type, $album_id) {
        include_once APP_PATH . "/framework/PHPExif/lib/PHPExif/Reader/Reader.php";

        // reader with Native adapter
        $reader = \PHPExif\Reader\Reader::factory(\PHPExif\Enum\ReaderType::NATIVE);
        $exif = $reader->read($raw);
        $exif_tags = [
            "Aperture",
            "Author",
            "Headline",
            "Credit",
            "Source",
            "Jobtitle",
            "Iso",
            "Exposure",
            "ExposureMilliseconds",
            "FocusDistance",
            "Width",
            "Height",
            "Title",
            "Caption",
            "Copyright",
            "Keywords",
            "Camera",
            "HorizontalResolution",
            "VerticalResolution",
            "Software",
            "FocalLength",
            "CreationDate",
            "ColorSpace",
            "MimeType",
            "FileSize",
            "FileName",
            "Orientation",
            "GPS",
            "Description",
            "Make",
            "Altitude",
            "Longitude",
            "Latitude",
            "ImgDirection",
            "Lens",
            "ContentIdentifier",
            "Framerate",
            "Duration",
            "MicroVideoOffset",
            "Sublocation",
            "City",
            "State",
            "Country"
        ];
        $exif_values = [];

        foreach($exif_tags as $attr) {
            $exif_values[strtolower($attr)] = $exif->{"get$attr"}();
        }

        $exif = json_encode($exif_values);
        $filename = $exif_values["filename"];
        $filename = "/" . date("Y") . "/" . substr($filename, 3, 3) . "/" . $filename;
        $upload = DotNetRegistry::Read("UPLOAD_DATA");
        $large = $upload . "/large" . $filename;
        $small = $upload . "/thumbs" . $filename;

        breakpoint([$upload_raw, $large, $small]);

        breakpoint($exif);
    }
}