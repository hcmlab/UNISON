<?php

function readplayerprop($id,$prop,$inst){
    $xml=simplexml_load_file("../instances/".$inst."/data/playerdata.xml") or die("Error: Cannot create object1");
    foreach($xml->player as $player) {
        if($player->rfid == $id) {
            return $player->$prop;
        }
    }
}
function readscale($inst,$no) {
    $xml=simplexml_load_file("../instances/".$inst."/data/scales.xml") or die("Error: Cannot create object2");
    return (int)$xml->item[$no-1];
}
function readgamestation($name,$inst,$item) {
    $xml=simplexml_load_file("../instances/".$inst."/data/gamedata.xml") or die("Error: Cannot create object3");
    $k = $xml->count();
    return $xml->round[$k-1]->$name->$item;
}
function readgameplayer($id,$inst,$item){
    $xml=simplexml_load_file("../instances/".$inst."/data/gamedata.xml") or die("Error: Cannot create object4");
    $k = $xml->count();
    foreach($xml->round[$k-1]->player as $player) {
        if($player->rfid == $id) {
            return $player->$item;
        }
    }    
}
function readgamefund($name,$inst) {
    $xml=simplexml_load_file("../instances/".$inst."/data/gamedata.xml") or die("Error: Cannot create object5");
    $k = $xml->count();
    return (int)$xml->round[$k-1]->$name;
}

function readinstances(){
    if(file_exists('../instances/instances.xml')) {
        $xml=simplexml_load_file("../instances/instances.xml");
        $array = [];
        foreach($xml->dir as $dir){
            $array[] = (string)$dir;
        }
        return json_encode($array);
    }
    else {
        return 'no instance database available';
    }
}

function readplayerdatabase($inst) {
    $xml=simplexml_load_file("../instances/".$inst."/data/playerdata.xml") or die("Error: Cannot create object6");
    $array = [];
    $i=0;
    foreach($xml->player as $player) {
        $array[$i] = [];
        $array[$i][1] = $player->rfid;
        $array[$i][0] = $player->name;
        $array[$i][2] = $player->mu;
        $array[$i][3] = $player->hp;
        $array[$i][6] = $player->ed;
        $array[$i][7] = $player->ls;
        $array[$i][5] = $player->sl;
        $array[$i][4] = $player->inf;
        $array[$i][8] = $player->pw;
        $array[$i][9] = $player->gender;
        $i +=1;
    }    
    return json_encode($array);
}

function readgamedatabase($inst) {
    $xml=simplexml_load_file("../instances/".$inst."/data/gamedata.xml") or die("Error: Cannot create object7");
    $array = [];
    $i = 0;
    foreach($xml->round as $round) {
        $array[$i] = [];
        $array[$i]['mall'] = $round->mall->inf;
        $array[$i]['lounge'] = $round->lounge->inf;
        $array[$i]['school'] = $round->school->inf;
        $array[$i]['townhall'] = $round->townhall->inf;
        $array[$i]['office'] = $round->office->inf;
        $array[$i]['vac'] = $round->vac;
        $array[$i]['stocks'] = $round->stocks;
        $array[$i]['taxamount'] = $round->taxamount;
        $array[$i]['incometax'] = $round->incometax;
        $array[$i]['propertytax'] = $round->propertytax;
        $array[$i]['highested'] = $round->highested;
        $j = 0;
        foreach($round->player as $player) {
            $array[$i][$j] = [];
            $array[$i][$j][0] = $player->rfid;
            $array[$i][$j][1] = $player->inf;
            $array[$i][$j][2] = $player->infstatus;
            $array[$i][$j][3] = $player->di;
            $array[$i][$j][4] = $player->stocks;
            $array[$i][$j][5] = $player->inoffice;
            $array[$i][$j][6] = $player->hp;
            $array[$i][$j][7] = $player->hps;
            $array[$i][$j][8] = $player->mu;
            $array[$i][$j][9] = $player->ed;
            $array[$i][$j][10] = $player->sl;
            $array[$i][$j][11] = $player->inhospital;
            $array[$i][$j][12] = $player->carepackage;
            $array[$i][$j][13] = $player->inquarantine;   
            $j += 1;
        }
        $i += 1;
    }
    return json_encode($array);

}
