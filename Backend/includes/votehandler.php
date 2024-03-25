<?php

function readvotetemp($inst) {
    $xml=simplexml_load_file("../instances/".$inst."/data/election.xml") or die("Error: Cannot create object");
    $array = [];
    foreach($xml->temp->vote as $vote) {
        $votearray = [];
        $votearray['text'] = $vote->text;
        if(isset($vote->textb)){
            $votearray['textb'] = $vote->textb;
        }
        $votearray['scale'] = $vote->scale;
        $votearray['valuetype'] = $vote->valuetype;
        $array[] = $votearray;
    }
    return json_encode($array);
}

function writevotedev($inst,$array2){
    $array = json_decode($array2);
    $xml=simplexml_load_file("../instances/".$inst."/data/election.xml") or die("Error: Cannot create object");
    $vote = $xml->dev->addChild('vote');
    $vote->addChild('text',$array[0]);
    
    //handling minimum wage problem
    if($array[1] == 15) {
        $value = $array[2];
        $of = (int)readscale($inst, 4);
        $minwg = 2*$of + $value;
        $maxwg =2*$of*((int) readgamefund('highested', $inst))-$value;
        if($minwg > $maxwg) {
            return 'minwage exceeds maxwage';
        }
    }
    
    $vote->addChild('scale',$array[1]);
    $vote->addChild('value',$array[2]);
    $k = $xml->dev->vote->count();
    $vote->id = $k;
    $vote->addChild('yes',0);
    $vote->addChild('no',0);
    $vote->addChild('status','inactive');
    file_put_contents('../instances/'.$inst.'/data/election.xml',$xml->saveXML());
    return $k;
}

function writevote($id,$inst,$voteid,$vote) {
    $xml=simplexml_load_file("../instances/".$inst."/data/election.xml") or die("Error: Cannot create object");
    $xml2=simplexml_load_file("../instances/".$inst."/data/playerdata.xml") or die("Error: Cannot create object");
    $i = true;
    foreach($xml2->player as $player) {
        if($player->rfid == $id){
            foreach($player->vote as $vote3) {
                if($vote3->id == $voteid) {
                    return 'player already voted';
                    $i = false;
                }
            }
            if($i){
                $vote4 = $player->addChild('vote');
                $vote4->addChild('id',$voteid);
                $vote4->addChild('vote',$vote);
                foreach($xml->dev->vote as $vote2) {
                    if($vote2->id == $voteid){
                        $vote2->$vote = (int)$vote2->$vote +1;
                    }
                }
            }
        }
    }    
    file_put_contents('../instances/'.$inst.'/data/election.xml',$xml->saveXML());
    file_put_contents('../instances/'.$inst.'/data/playerdata.xml',$xml2->saveXML());
}

function closevote($inst,$voteid){
    $xml=simplexml_load_file("../instances/".$inst."/data/election.xml") or die("Error: Cannot create object");
    $j = false;
    foreach($xml->dev->vote as $vote) {
        if($vote->id == $voteid){
            $text = $vote->text;
            $vote->status = 'closed';
            $yes = (int)$vote->yes;
            $no = (int)$vote->no;
            if($yes > $no){
                foreach($vote->scales->scale as $scale) {
                    $number = (int)$scale->number;
                    $value = (int)$scale->value;
                    writescale($inst, $number, $value);
                }
                $j = true;
            }
            else{
                $j = false;
            }
            
        }
    }
    $k = 1;
    while(file_exists('../instances/'.$inst.'/news/info'.$k.'.php')) {
        $k +=1;
    }
    $handle = file_get_contents('../instances/voteresult.php');
    $str = str_replace('zjznudfnsrzabezntum','plotmaker("<?php echo plotmaker2('.$voteid.'); ?>","'.$text.'")',$handle);
    file_put_contents('../instances/'.$inst.'/news/info'.$k.'.php', $str);
    file_put_contents('../instances/'.$inst.'/data/election.xml',$xml->saveXML());
    return $j;
}

function openvote($inst,$voteid){
    $xml=simplexml_load_file("../instances/".$inst."/data/election.xml") or die("Error: Cannot create object");
    foreach($xml->dev->vote as $vote) {
        if($vote->id == $voteid){
            $vote->status = 'open';
        }
    }
    file_put_contents('../instances/'.$inst.'/data/election.xml',$xml->saveXML());
    return 'true';    
}

function listopenvote($inst,$voter){
    $xml=simplexml_load_file("../instances/".$inst."/data/election.xml") or die("Error: Cannot create object");
    $xml2=simplexml_load_file("../instances/".$inst."/data/playerdata.xml") or die("Error: Cannot create object");
    $array = [];
    foreach($xml->dev->vote as $vote){
        if($vote->status == 'open'){
            $alreadyvoted = true;
            foreach($xml2->player as $player) {
                if($player->rfid == $voter) {
                    foreach($player->vote as $blah) {
                        if((string)$blah->id == (string)$vote->id) {
                            $alreadyvoted = false;
                        }
                    }
                }
            }
            if($alreadyvoted) {
                $votearray = [];
                $votearray['id'] = (int)$vote->id;
                $votearray['text'] = $vote->text;
                $array[] = $votearray;
            }
        }
    }
    return json_encode($array);
}
function listvote($inst){
    $xml=simplexml_load_file("../instances/".$inst."/data/election.xml") or die("Error: Cannot create object");
    $array = [];
    foreach($xml->dev->vote as $vote){
        $votearray = [];
        $votearray['id'] = (int)$vote->id;
        $votearray['status'] = $vote->status;
        $votearray['text'] = $vote->text;
        $array[] = $votearray;  
    }
    return json_encode($array);
}
function delvote($inst,$voteid){
    $xml=simplexml_load_file("../instances/".$inst."/data/election.xml") or die("Error: Cannot create object");
    foreach($xml->dev->vote as $vote){
        if($vote->id == $voteid){
            $dom = dom_import_simplexml($vote);
            $dom->parentNode->removeChild($dom);
        }
    }
    file_put_contents('../instances/'.$inst.'/data/election.xml',$xml->saveXML());
    return 'true';
}