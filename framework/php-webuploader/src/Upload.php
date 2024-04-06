<?php

class Upload {

    /**
     * 上传接口
     * $targetDir 切片保留路径(可自行修改)
     * $uploadDir 最终上传路径(可自行修改)
     * $domain   域名(可自定义修改)
     * $urls     返回地址(可自定义修改)
     * @return mixed
     */
    public function uploadVideo($upload = "/upload/", $upload_temp = "/tmp/upload_temp/")
    {
        // Make sure file is not cached (as it happens for example on iOS devices)
        header("Access-Control-Allow-origin:*");
        //header("Access-Control-Allow-Credentials:true");
        //header('Access-Control-Allow-Headers:x-requested-with,content-type');
        header("Expires: Mon, 26 Jul 1997 05:00:00 GMT");
        header("Last-Modified: ".gmdate("D, d M Y H:i:s")." GMT");
        header("Cache-Control: no-store, no-cache, must-revalidate");
        header("Cache-Control: post-check=0, pre-check=0", false);
        header("Pragma: no-cache");

        // Support CORS
        // header("Access-Control-Allow-Origin: *");
        // other CORS headers if any...
        if ($_SERVER['REQUEST_METHOD'] == 'OPTIONS') {
            exit; // finish preflight CORS requests here
        }

        if (!empty($_REQUEST['debug'])) {
            $random = rand(0, intval($_REQUEST['debug']));
            if ($random === 0) {
                header("HTTP/1.0 500 Internal Server Error");
                exit;
            }
        }
        //
        //var_dump($_REQUEST);
        //
        // header("HTTP/1.0 500 Internal Server Error");
        // exit;

        // 30 minutes execution time
        @set_time_limit(30 * 60);

        $targetDir = $upload_temp;             //切片保留路径(可自行修改["名称+_temp"])
        $uploadDir = $upload. '/'.date('Ymd');     //最终上传路径(可自行修改)

        $cleanupTargetDir = true; // Remove old files
        $maxFileAge       = 5 * 3600; // Temp file age in seconds


        // Create target dir
        if (!file_exists($targetDir)) {
            @mkdir($targetDir, 0777, true);
        }

        // Create target dir
        if (!file_exists($uploadDir)) {
            @mkdir($uploadDir, 0777, true);
        }

        // Get a file name
        if (isset($_REQUEST["name"])) {
            $fileName = $_REQUEST["name"];
        } elseif (!empty($_FILES)) {
            $fileName = $_FILES["file"]["name"];
        } else {
            $fileName = uniqid("file_");
        }
        $ext = strrchr($fileName, '.');

        $times     = md5(microtime(true));
        $fileNames = $times.$ext;

        $filePath = str_replace('\\', '/', $targetDir.DIRECTORY_SEPARATOR.$fileName);
        // $uploadPath = $uploadDir . DIRECTORY_SEPARATOR . $fileNames;
        $uploadPath = str_replace('\\', '/', $uploadDir.DIRECTORY_SEPARATOR.$fileNames);

        $domain = '';//配置域名可自行修改
        $urls   = $domain.'/'.$uploadPath;

        // Chunking might be enabled
        $chunk  = isset($_REQUEST["chunk"]) ? intval($_REQUEST["chunk"]) : 0;
        $chunks = isset($_REQUEST["chunks"]) ? intval($_REQUEST["chunks"]) : 1;

        // Remove old temp files
        if ($cleanupTargetDir) {
            if (!is_dir($targetDir) || !$dir = opendir($targetDir)) {
                // echo '123';
                die('{"jsonrpc" : "2.0", "error" : {"code": 100, "message": "Failed to open temp directory."}, "id" : "id","uploadPath":$uploadPath}');
            }

            while (($file = readdir($dir)) !== false) {
                $tmpfilePath = $targetDir.DIRECTORY_SEPARATOR.$file;

                // If temp file is current file proceed to the next
                if ($tmpfilePath == "{$filePath}_{$chunk}.part" || $tmpfilePath == "{$filePath}_{$chunk}.parttmp") {
                    continue;
                }

                // Remove temp file if it is older than the max age and is not the current file
                if (preg_match('/\.(part|parttmp)$/', $file) && (@filemtime($tmpfilePath) < time() - $maxFileAge)) {
                    @unlink($tmpfilePath);
                }
            }
            closedir($dir);
        }


        // Open temp file
        if (!$out = @fopen("{$filePath}_{$chunk}.parttmp", "wb")) {
            die('{"jsonrpc" : "2.0", "error" : {"code": 102, "message": "Failed to open output stream."}, "id" : "id"}');
        }

        if (!empty($_FILES)) {
            if ($_FILES["file"]["error"] || !is_uploaded_file($_FILES["file"]["tmp_name"])) {
                die('{"jsonrpc" : "2.0", "error" : {"code": 103, "message": "Failed to move uploaded file."}, "id" : "id"}');
            }

            // Read binary input stream and append it to temp file
            if (!$in = @fopen($_FILES["file"]["tmp_name"], "rb")) {
                die('{"jsonrpc" : "2.0", "error" : {"code": 101, "message": "Failed to open input stream."}, "id" : "id"}');
            }
        } else {
            if (!$in = @fopen("php://input", "rb")) {
                die('{"jsonrpc" : "2.0", "error" : {"code": 101, "message": "Failed to open input stream."}, "id" : "id"}');
            }
        }

        while ($buff = fread($in, 4096)) {
            fwrite($out, $buff);
        }

        @fclose($out);
        @fclose($in);

        $chunk_part_temp = "{$filePath}_{$chunk}.part";

        rename("{$filePath}_{$chunk}.parttmp", $chunk_part_temp);

        $index = 0;
        $done  = true;
        for ($index = 0; $index < $chunks; $index++) {
            if (!file_exists("{$filePath}_{$index}.part")) {
                $done = false;
                break;
            }
        }
        if ($done) {
            if (!$out = @fopen($uploadPath, "wb")) {
                // echo '1';
                die('{"jsonrpc" : "2.0", "error" : {"code": 102, "message": "Failed to open output stream."}, "id" : "id"}');
            }

            if (flock($out, LOCK_EX)) {
                for ($index = 0; $index < $chunks; $index++) {
                    if (!$in = @fopen("{$filePath}_{$index}.part", "rb")) {
                        break;
                    }

                    while ($buff = fread($in, 4096)) {
                        fwrite($out, $buff);
                    }

                    @fclose($in);
                    @unlink("{$filePath}_{$index}.part");
                }

                flock($out, LOCK_UN);
            }
            @fclose($out);
        }

        exit(json_encode(['code' => 200, 'msg' => 'ok', 'data' => [
            "dir" => \basename(\dirname($uploadPath)),
            "name" => \basename($uploadPath)
        ]]));
    }
}


