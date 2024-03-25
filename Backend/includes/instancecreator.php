<?php
function instancecreator($inst) {
    $xml=simplexml_load_file("../instances/rawdata/playerdata.xml") or die("Error: No players registered yet");
    if ( !is_dir( '../instances/'.$inst ) ) {
        mkdir('../instances/'.$inst);
        mkdir('../instances/'.$inst.'/data/');
        mkdir('../instances/'.$inst.'/logs/');
        mkdir('../instances/'.$inst.'/news/');
        file_put_contents('../instances/'.$inst.'/data/playerdata.xml',$xml->saveXML());
        if(file_exists('../instances/instances.xml')) {
            $xml2=simplexml_load_file("../instances/instances.xml");
            $xml2->addChild('dir',$inst);
            file_put_contents("../instances/instances.xml",$xml2->saveXML());
            echo "true";
        }
        else {
            echo 'instance database not available';
        }
    }
    else {
        echo 'this instance already exists';
    }

}
