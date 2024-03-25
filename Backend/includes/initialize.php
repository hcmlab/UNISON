<?php

function initialize($inst){
    $xml=simplexml_load_file("../instances/".$inst."/data/playerdata.xml") or die("Error: Cannot create object");
    $xml2=simplexml_load_file("../instances/rawdata/gamedata_raw.xml") or die("Error: Cannot create object");
    $xml3=simplexml_load_file("../instances/rawdata/scales_raw.xml") or die("Error: Cannot create object");
    $xml4=simplexml_load_file("../instances/rawdata/plotdata_raw.xml") or die("Error: Cannot create object");
    $xml5=simplexml_load_file("../instances/rawdata/election_raw.xml") or die("Error: Cannot create object");
    $heute = getdate();
    if($heute['minutes']<10){
        $datum = $heute['mday'].$heute['month'].$heute['year'].$heute['hours']."0".$heute['minutes'];
    }
    else {
        $datum = $heute['mday'].$heute['month'].$heute['year'].$heute['hours'].$heute['minutes'];
    }
    
//    archive
//    mkdir("../instances/".$inst.'/data/archive/'.$datum);
//    $dump1 = simplexml_load_file("../instances/".$inst."/data/gamedata.xml") or die("Error: Cannot create object");
//    $dump2 = simplexml_load_file("../instances/".$inst."/data/scales.xml") or die("Error: Cannot create object");
//    $dump3 = simplexml_load_file("../instances/".$inst."/data/plotdata.xml") or die("Error: Cannot create object");
//    $dump4 = simplexml_load_file("../instances/".$inst."/data/election.xml") or die("Error: Cannot create object");
//    file_put_contents('../instances/'.$inst.'/data/archive/'.$datum.'/gamedata.xml',$dump1->saveXML());
//    file_put_contents('../instances/'.$inst.'/data/archive/'.$datum.'/scales.xml',$dump2->saveXML());
//    file_put_contents('../instances/'.$inst.'/data/archive/'.$datum.'/plotdata.xml',$dump3->saveXML());
//    file_put_contents('../instances/'.$inst.'/data/archive/'.$datum.'/election.xml',$dump4->saveXML());
    
    // create first round in gamedata
    $xml2->addChild('round');
    $mall = $xml2->round->addChild('mall');
    $mall->addChild('inf',0);
    $lounge = $xml2->round->addChild('lounge');
    $lounge->addChild('inf',0);
    $school = $xml2->round->addChild('school');
    $school->addChild('inf',0);
    $townhall = $xml2->round->addChild('townhall');
    $townhall->addChild('inf',0);
    $office = $xml2->round->addChild('office');
    $office->addChild('inf',0);    
    $xml2->round->addChild('vac',0);
    $xml2->round->addChild('stocks',0);
    $xml2->round->addChild('taxamount',0);
    $xml2->round->addChild('incometax',0);
    $xml2->round->addChild('propertytax',0); 
    $xml2->round->addChild('highested',1);
    
    //initialize player properties
    $i = 0;
    $infmfd = 0;
    $mon = 0;
    $hps = 0;
    function nrand($mean, $sd){
        $x = mt_rand()/mt_getrandmax();
        $y = mt_rand()/mt_getrandmax();
        return floor(sqrt(-2*log($x))*cos(2*pi()*$y)*$sd + $mean);
    }
    foreach($xml->player as $key){
        $rfid = $key->rfid;
        $xml2->round->addChild('player');
        $xml2->round->player[$i]->addChild('rfid');
        $xml2->round->player[$i]->rfid = $rfid;
        $xml2->round->player[$i]->addChild('inf',0);
        $xml2->round->player[$i]->addChild('infstatus');

        $rnd1 = nrand(40,15);
        $rnd2 = nrand(40,15);
        $rnd3 = nrand(50,15);
        if($rnd1 < 10) {
            $rnd1 = 10;
        }
        if($rnd2 < 10) {
            $rnd2 = 10;
        }
        $key->mu = $rnd1;
        $key->hp = $rnd2;
        $key->ls = $rnd3/100;
        $key->ed = 1;
        $key->sl = 0;
        $mon += $key->mu;
        $hps += $key->hp;
        $prob = (int)$xml3->item[5];
        $inf = rand(0,$prob);
        if($inf == 0) {
            $key->inf = 1;
            $xml2->round->player[$i]->infstatus = 1;
            $infmfd += 1;
        }
        else {
            $key->inf = 0;
            $xml2->round->player[$i]->infstatus = 0;        
        }
        $xml2->round->player[$i]->addChild('di',0);
        $xml2->round->player[$i]->addChild('stocks',0);
        $xml2->round->player[$i]->addChild('inoffice',0);
        $xml2->round->player[$i]->addChild('hp',0);
        $xml2->round->player[$i]->addChild('hps',$rnd2);
        $xml2->round->player[$i]->addChild('mu',$rnd1);
        $xml2->round->player[$i]->addChild('ed',1);
        $xml2->round->player[$i]->addChild('sl',0);
        $xml2->round->player[$i]->addChild('inhospital',0);
        $xml2->round->player[$i]->addChild('carepackage',0);
        $xml2->round->player[$i]->addChild('inquarantine',1);
        $xml2->round->player[$i]->addChild('boughttest',0);
        $i +=1;
    }
    
    //plot data initialize
    $xml4->infection->addChild('round',$infmfd);
    $xml4->stocks->addChild('round',0);
    $xml4->money->addChild('round',$mon);
    $xml4->health->addChild('round',$hps);
    $xml4->vaccine->addChild('round',0);
    $xml4->hospital->addChild('round',0);
    
    //copy news files
    copy("../instances/rawdata/news/info1.php","../instances/".$inst."/news/info1.php");
    copy("../instances/rawdata/news/info2.php","../instances/".$inst."/news/info2.php");
    copy("../instances/rawdata/news/info3.php","../instances/".$inst."/news/info3.php");
    copy("../instances/rawdata/news/info4.php","../instances/".$inst."/news/info4.php");
    copy("../instances/rawdata/news/info5.php","../instances/".$inst."/news/info5.php");
    copy("../instances/rawdata/news/info6.php","../instances/".$inst."/news/info6.php");
    copy("../instances/rawdata/news/banderole.txt","../instances/".$inst."/news/banderole.txt");
    copy("../instances/rawdata/news/inhospital.txt","../instances/".$inst."/news/inhospital.txt");
    copy("../instances/rawdata/news/news_glob.php","../instances/".$inst."/news/news_glob.php");
    copy("../instances/rawdata/news/news_ind.php","../instances/".$inst."/news/news_ind.php");
    mkdir("../instances/".$inst."/pvq");
    mkdir("../instances/".$inst."/pvq/language");
    mkdir("../instances/".$inst."/pvq/language/spanish");
    mkdir("../instances/".$inst."/pvq/language/english");
    copy("../instances/pvq/language/english/questions_male.xml","../instances/".$inst."/pvq/language/english/questions_male.xml");
    copy("../instances/pvq/language/english/questions_female.xml","../instances/".$inst."/pvq/language/english/questions_female.xml");
    copy("../instances/pvq/language/english/questions_diverse.xml","../instances/".$inst."/pvq/language/english/questions_diverse.xml");
    copy("../instances/pvq/language/spanish/questions_male.xml","../instances/".$inst."/pvq/language/spanish/questions_male.xml");
    copy("../instances/pvq/language/spanish/questions_female.xml","../instances/".$inst."/pvq/language/spanish/questions_female.xml");
    copy("../instances/pvq/language/spanish/questions_diverse.xml","../instances/".$inst."/pvq/language/spanish/questions_diverse.xml");    
    
    
    file_put_contents("../instances/".$inst."/data/playerdata.xml",$xml->saveXML());
    file_put_contents("../instances/".$inst."/data/gamedata.xml",$xml2->saveXML());
    file_put_contents("../instances/".$inst."/data/scales.xml",$xml3->saveXML());
    file_put_contents("../instances/".$inst."/data/plotdata.xml",$xml4->saveXML());
    file_put_contents("../instances/".$inst."/data/election.xml",$xml5->saveXML());
    return 'true';
}