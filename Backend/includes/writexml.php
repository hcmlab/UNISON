<?php

function writeplayerprop($id,$inst,$prop,$value){
    $xml=simplexml_load_file("../instances/".$inst."/data/playerdata.xml") or die("Error: Cannot create object");
    foreach($xml->player as $player) {
        if($player->rfid == $id) {
            $player->$prop = $value;
        }
    }
    file_put_contents('../instances/'.$inst.'/data/playerdata.xml',$xml->saveXML());  
}

function writegameplayer($id,$inst,$item,$value) {
    $xml=simplexml_load_file("../instances/".$inst."/data/gamedata.xml") or die("Error: Cannot create object");
    $k = $xml->count();
    foreach($xml->round[$k-1]->player as $player) {
        if($player->rfid == $id) {
            $player->$item = $value;
        }
    }
    file_put_contents('../instances/'.$inst.'/data/gamedata.xml',$xml->saveXML());  
}
function writegamestation($name,$inst,$prop,$value) {
    $xml=simplexml_load_file("../instances/".$inst."/data/gamedata.xml") or die("Error: Cannot create object");
    $k = $xml->count();
    
    if($value == 1) {
        $xml->round[$k-1]->$name->$prop = ((float)$xml->round[$k-1]->$name->$prop)/5+1;
    }
    else {
        $xml->round[$k-1]->$name->$prop = ((float)$xml->round[$k-1]->$name->$prop)/5;
    }
    file_put_contents('../instances/'.$inst.'/data/gamedata.xml',$xml->saveXML());  
    return $value;
}
function writegamefund($name,$inst,$value) {
    $xml=simplexml_load_file("../instances/".$inst."/data/gamedata.xml") or die("Error: Cannot create object");
    $k = $xml->count();   
    $xml->round[$k-1]->$name = $value;
    file_put_contents('../instances/'.$inst.'/data/gamedata.xml',$xml->saveXML()); 
}

function writescale($inst,$no,$value){
    $xml=simplexml_load_file("../instances/".$inst."/data/scales.xml") or die("Error: Cannot create object");
    $xml->item[$no-1] = $value;
    file_put_contents('../instances/'.$inst.'/data/scales.xml',$xml->saveXML()); 
}

