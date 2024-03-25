<?php

function register($id,$name,$inst,$pw) {
    $xml=simplexml_load_file("../instances/".$inst."/data/playerdata.xml") or die("Error: Cannot create object");
    $a = false;
    foreach ($xml->player as $player){
        if($player->rfid == $id) {
            $str = 'id already in use';
            $a = true;
        }
        if($player->name == $name) {
            $str = 'name already in use';
            $a = true;
        }
    }
    if($a){
        return $str;
    }
    else {
        $newplayer = $xml->addChild('player');
        $newplayer->addChild('rfid',$id);
        $newplayer->addChild('name',$name);
        $newplayer->addChild('pw',$pw);
        $newplayer->addChild('mu',0);
        $newplayer->addChild('hp',0);
        $newplayer->addChild('ed',1);
        $newplayer->addChild('ls',0);
        $newplayer->addChild('sl',0);
        $newplayer->addChild('inf',0);
        file_put_contents('../instances/'.$inst.'/data/playerdata.xml',$xml->saveXML());  
        return 'true';
    }
}