<?php

function nextround($inst){
    $xml=simplexml_load_file("../instances/".$inst."/data/playerdata.xml") or die("Error: Cannot create object");
    $xml2=simplexml_load_file("../instances/".$inst."/data/gamedata.xml") or die("Error: Cannot create object");
    $xml3=simplexml_load_file("../instances/".$inst."/data/scales.xml") or die("Error: Cannot create object");
    $xml4=simplexml_load_file("../instances/".$inst."/data/plotdata.xml") or die("Error: Cannot create object");
    require_once 'readxml.php';
    
    //create new round
    $newround = $xml2->addChild('round');
    $mall = $newround->addChild('mall');
    $mall->addChild('inf',0);
    $lounge = $newround->addChild('lounge');
    $lounge->addChild('inf',0);
    $school = $newround->addChild('school');
    $school->addChild('inf',0);
    $townhall = $newround->addChild('townhall');
    $townhall->addChild('inf',0);
    $office = $newround->addChild('office');
    $office->addChild('inf',0);    
    
    //global funds & taxes
    $vacamount = (int) readgamefund('vac', $inst);
    $stocksamount = (int) readgamefund('stocks', $inst);
    $taxamount = (int) readgamefund('taxamount', $inst);
    if($taxamount < 0){
        $k = $xml->count();
        $tx = round($taxamount/$k,0);
        $taxamount = 0;
    }
    else {
        $tx = 0;
    }
    $incometax = (int) readscale($inst, 10);
    $propertytax = (int) readscale($inst, 11);
    $newround->addChild('vac',$vacamount);
    $newround->addChild('stocks',$stocksamount);
    $taxamountnew = $newround->addChild('taxamount',$taxamount);
    $taxes = (int)$taxamountnew;
    $newround->addChild('incometax',$incometax);
    $newround->addChild('propertytax',$propertytax);
    
    
    //misc
    $highested = readgamefund('highested', $inst);
    $newround->addChild('highested',$highested);
    
    //update office factor
    $growth = (int) readscale($inst, 9);
    $officefactor = (int) readscale($inst, 4);
    $newofficefactor = $officefactor + $stocksamount/$growth;
    $xml3->item[4] = $newofficefactor;
    
    //check for vaccine
    $dice3 = rand(1,1000);
    $vacprob = (pow(($vacamount/4000),2)*1000);
    $vac = true;
    if($dice3 < $vacprob){
        return 'Vaccine was found';
        $vac = false;
    }
    
    //update player properties
    $i = 0;
    $infmfd = 0;
    $mon = 0;
    $hps = 0;
    $icu = 0;
    
    $inhospitaltxt = fopen('../instances/'.$inst.'/news/inhospital.txt','w');
    $str = "Los siguientes jugadores están actualmente hospitalizados: \n\r";

    foreach($xml->player as $key){
        $rfid = (string)$key->rfid;
        $newround->addChild('player');
        $newround->player[$i]->addChild('rfid',$rfid);
        $newround->player[$i]->addChild('inf',0);
        $newround->player[$i]->addChild('inhospital',0);
        $inquarantine = (int)readgameplayer($rfid, $inst, 'inquarantine');
        $newround->player[$i]->addChild('inquarantine',1);
       
        //HP and Hospital handling
        $hospitaldays = (int)readgameplayer($rfid, $inst, 'inhospital');
        if($hospitaldays == 0){
            if($inquarantine == 0) {
                $key->hp = (int)$key->hp - (int)readscale($inst, 7) - (int)$key->sl;
                if($key->inf == 1){
                    $key->hp = (int)$key->hp - (int)readscale($inst, 18) ;
                }
                $zwischenlager = (int)$key->hp;
                if($zwischenlager < 0){
                    $str = $str.(string)$key->name."\n\r";
                    $insurance = (int)readscale($inst, 12);
                    if($insurance == 1) {
                        $newround->player[$i]->inhospital = 1;
                        $taxes -= (int)readscale($inst, 20);
                    }
                    else {
                        $newround->player[$i]->inhospital = 2;
                    }
                    $icu += 1;
                    $newround->player[$i]->inquarantine = 0;
                }
            }
        }
        else if ($hospitaldays == 1) {
            
            $newround->player[$i]->inhospital = 0;
            $key->hp = 10;
            $key->inf = 0;
            $key->sl = round(((int)$key->sl)/2,0);
            $newround->player[$i]->addChild('infstatus',0);
            $newround->player[$i]->inquarantine = 0;
        }
        else {
            $str = $str.(string)$key->name."\n\r";
            $newround->player[$i]->inhospital = 1;
            $icu += 1;
        }
        $hps += (int)$key->hp;
        $newround->player[$i]->addChild('hps',(int)$key->hp);

        
        //infection handling
        $infprob = (int)readgameplayer($rfid, $inst, 'inf');
        $dice = rand(1,1000);
        $di = (int) readgameplayer($rfid, $inst, 'di');
        $key->inf = 0;
        if($di == 0 && $key->inf == 0 && $dice < $infprob*1000){
            $key->inf = 1;
            $newround->player[$i]->addChild('infstatus',1);
            $infmfd += 1;
        }
        if($inquarantine == '1') {
            $key->inf = 0;
        }
        $newround->player[$i]->addChild('infstatus',(int)$key->inf);        
        
        //money handling
        //stocks & dividend & taxamount distribution
        $stocks = (int)readgameplayer($rfid, $inst, 'stocks');
        $newround->player[$i]->addChild('stocks',$stocks);
        $dividend = (int)readscale($inst, 8);
        $newmu = round($stocks*$dividend/100,0);
        $newmutaxes = round($newmu*$incometax/100,0);
        $mu = (int)$key->mu;
        $mu += $newmu-$newmutaxes+$tx;
        $taxes += $newmutaxes;
        // propertytax
        $propertytaxpayment = round(((int)$key->mu)*$propertytax/100,0);
        $mu -= $propertytaxpayment;
        $taxes += $propertytaxpayment;
        //disappropriation
        $disapp = (int)readscale($inst, 16);
        if($disapp > 0 && $mu > $disapp) {
            $taxes += $mu - $disapp;
            $mu = $disapp;
        }
        //social dafety
        $socsaf = readscale($inst,17);
        if($socsaf > 0 && $mu < 0) {
            $taxes += $mu-10;
            $mu = 10;
        }
        //final money handling
        $newround->player[$i]->addChild('mu',$mu);
        $key->mu = $mu;
        $mon += $mu;
        
        //reset other game properties
        $newround->player[$i]->addChild('di',0);
        $newround->player[$i]->addChild('inoffice',0);
        $newround->player[$i]->addChild('hp',0);
        $newround->player[$i]->addChild('carepackage',0);
        $newround->player[$i]->addChild('ed',(float)$key->ed);
        $newround->player[$i]->addChild('sl',(int)$key->sl);
        $newround->player[$i]->addChild('boughttest',0);
        $i += 1;
    }
    fwrite($inhospitaltxt,$str);
    fclose($inhospitaltxt);
    
    
    //update tax amount
    $newround->taxamount = $taxes;
    
    //infection if all are cured
    if($infmfd == 0 && $vac){
        foreach($xml->player as $player){
            $rnd = rand(1,10);
            if($rnd == 1) {
                $player->inf = 1;
                $rfid = $player->rfid;
                foreach($newround->player as $key) {
                    if((string)$key->rfid == (string)$rfid) {
                        $key->inf = 1;
                    }
                }
            }
        }
    }
    
    
    
    //plodata update
    $xml4->infection->addChild('round',$infmfd);
    $xml4->stocks->addChild('round',$stocksamount);
    $xml4->money->addChild('round',$mon);
    $xml4->health->addChild('round',$hps);
    $xml4->hospital->addChild('round',$icu);
    $xml4->vaccine->addChild('round',$vacamount);

    file_put_contents("../instances/".$inst."/data/playerdata.xml",$xml->saveXML());
    file_put_contents("../instances/".$inst."/data/gamedata.xml",$xml2->saveXML());
    file_put_contents("../instances/".$inst."/data/scales.xml",$xml3->saveXML());
    file_put_contents("../instances/".$inst."/data/plotdata.xml",$xml4->saveXML());
    
    return 'true';
}