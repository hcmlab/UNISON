<?php

function plotmaker($item) {
    $xml=simplexml_load_file("../data/plotdata.xml") or die("Error: Cannot create object");
    $array = [];
    foreach($xml->$item->round as $round) {
        $array[] = (int)$round;
    }

    return json_encode($array);
}

function plotmaker2($voteid) {
    $xml=simplexml_load_file("../data/election.xml") or die("Error: Cannot create object");
    $array = [];
    foreach($xml->dev->vote as $vote) {
        if($vote->id == $voteid) {
            $array = [(int)$vote->yes,(int)$vote->no];
        }
    }
    return json_encode($array);
}
?>