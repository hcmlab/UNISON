<?php

if (isset($_REQUEST['ac'])) {
    $ac = $_REQUEST['ac'];
    $inst = $_REQUEST['inst'];
    $str = '';
    include 'readxml.php';
    include 'writexml.php';
    include 'votehandler.php';
    $server = $_SERVER['HTTP_HOST'];
    include 'logwriter.php';
    if($ac == 'm01'){
        //visit the mall
        if(isset($_REQUEST['value1'])) {
            $id = $_REQUEST['value1'];
            $str = $id;
            $inhospital = (int)readgameplayer($id, $inst, 'inhospital');
            if($inhospital > 0) {
                echo 'player is in hospital';
            }
            else {
                $inf = readplayerprop($id, 'inf', $inst);
                if($inf == 1){
                    writegamestation('mall',$inst,'inf','1');
                }
                else {
                    $infprob = readgamestation('mall',$inst,'inf');
                    writegamestation('mall',$inst,'inf',0);
                    $infprob2 = (int) readgameplayer($id, $inst, 'inf');
                    writegameplayer($id, $inst, 'inf',$infprob+$infprob2);
                }
                writegameplayer($id, $inst, 'inquarantine', 0);
                echo 'true';
            }
        }
        else {
            echo "Value missing";
        }
    }
    if($ac == 'm03') {
        //buy disinfectant
        if(isset($_REQUEST['value1'])) {        
            $id = $_REQUEST['value1'];
            $str = $id;
            $cost = (int)readscale($inst,1);
            $mu = (int)readplayerprop($id,'mu',$inst);
            if($cost > $mu){
                echo 'Insufficient money';
            }
            else {   
                writeplayerprop($id,$inst,'mu',$mu - $cost);
                writegameplayer($id,$inst,'di',1);
                echo 'true';
            }
        }
        else {
            echo "Value missing";
        }              
    }
    if($ac == 'm04'){
        //buy health points
        if(isset($_REQUEST['value1'])) { 
            $id = $_REQUEST['value1'];
            $str = '['.$id.']';
            $cost = (int)readscale($inst,2);
            $mu = (int)readplayerprop($id,'mu',$inst);
            if($cost > $mu) {
                echo 'Insufficient money';
            }
            else {
                writeplayerprop($id,$inst,'mu',$mu - $cost);
                $hp = (int)readplayerprop($id,'hp',$inst);
                $hpcounter = (int)readgameplayer($id, $inst, 'hp');
                $hpadd = 10/pow(2,$hpcounter);
                writegameplayer($id, $inst, 'hp', $hpcounter +1);
                writeplayerprop($id,$inst,'hp',$hp + $hpadd);
                echo $hpcounter;
            }  
        }
        else {
            echo "Value missing";
        }           
    }
    if($ac == 'm05') {
        //buy health check
        if(isset($_REQUEST['value1'])) { 
            $id = $_REQUEST['value1'];
            $str = '['.$id.']';
            $cost = (int)readscale($inst,3);
            $mu = (int)readplayerprop($id,'mu',$inst);
            if($cost > $mu){
                echo 'Insufficient money';
            }
            else {   
                writeplayerprop($id,$inst,'mu',$mu - $cost);
                $hp = (int)readplayerprop($id,'hp',$inst);
                $sl = (int)readplayerprop($id,'sl',$inst);
                $inf = (int)readplayerprop($id,'inf',$inst);
                writeplayerprop($id, $inst, 'boughttest', 1);
                echo json_encode(array('hp' => $hp,'sl'=> $sl,'inf'=>$inf,));
            }
        }
        else {
            echo "Value missing";
        }         
    }
    if($ac == 'm06') {
        //invest into vaccine
        if(isset($_REQUEST['value1'])) { 
            $id = $_REQUEST['value1'];
            $cost = $_REQUEST['value2'];
            $str = '['.$id.','.$cost.']';
            $mu = (int)readplayerprop($id,'mu',$inst);
            if($cost > $mu) {
                echo 'Insufficient money';
            }
            else {
                writeplayerprop($id,$inst,'mu',$mu - $cost);
                $vacfund = readgamefund('vac',$inst);
                writegamefund('vac',$inst,$vacfund+$cost);
                echo 'true';
            }
        }
        else {
            echo "Value missing";
        }               
    }
    if($ac == 'm07') {
        //invest into stocks
        if(isset($_REQUEST['value1'])) { 
            $id = $_REQUEST['value1'];
            $cost = $_REQUEST['value2'];
            $str = '['.$id.','.$cost.']';
            $mu = (int)readplayerprop($id,'mu',$inst);
            if($cost > $mu) {
                echo 'Insufficient money';
            }
            else {
                writeplayerprop($id,$inst,'mu',$mu - $cost);
                $vacfund = readgamefund('stocks',$inst);
                writegamefund('stocks',$inst,$vacfund+$cost);
                $stocks = readgameplayer($id, $inst, 'stocks');
                writegameplayer($id, $inst, 'stocks', $stocks+$cost);
                echo 'true';
            } 
        }
        else {
            echo "Value missing";
        }           
    }
    if($ac == 'm08'){
        //send money
        if(isset($_REQUEST['value1']) && isset($_REQUEST['value2']) && isset($_REQUEST['value3'])) { 
            $id1 = $_REQUEST['value1'];
            $id2 = $_REQUEST['value2'];
            $amount = $_REQUEST['value3'];
            $str = '['.$id1.','.$id2.','.$amount.']';
            $mu1 = (int)readplayerprop($id1,'mu',$inst);
            $mu2 = (int)readplayerprop($id2,'mu',$inst);
            if($amount > $mu1) {
                echo 'Insufficient money';
            }
            else {
                writeplayerprop($id1,$inst,'mu',$mu1 - $amount);
                writeplayerprop($id2,$inst,'mu',$mu2 + $amount);
                echo 'true';
            }
        }
        else {
            echo "Value missing";
        }             
    }
    if($ac == 'm99'){
        if(isset($_REQUEST['value1'])) {
            $id = $_REQUEST['value1'];
            $str = '['.$id.']';
            echo 'true';
        }
    }
    if($ac == 'o01'){
        //visit office
        if(isset($_REQUEST['value1'])) { 
            $id = $_REQUEST['value1'];
            $str = '['.$id.']';
            $inhospital = readgameplayer($id, $inst, 'inhospital');
            if($inhospital > 0) {
                echo 'player is in hospital';
            }
            else {
                $inf = readplayerprop($id, 'inf', $inst);
                if($inf == 1){
                    writegamestation('office',$inst,'inf',1);
                }
                else {
                    $infprob = readgamestation('office',$inst,'inf');
                    writegamestation('office',$inst,'inf',0);
                    $infprob2 = (int) readgameplayer($id, $inst, 'inf');
                    writegameplayer($id, $inst, 'inf',$infprob+$infprob2);
                }
                writegameplayer($id, $inst, 'inquarantine', 0);
                echo 'true';
            }
        }
        else {
            echo "Value missing";
        }                
    }
    if($ac == 'o02') {
        //earning money
        if(isset($_REQUEST['value1'])) { 
            $id = $_REQUEST['value1'];
//            $inoffice = readgameplayer($id, $inst, 'inoffice');
//            if($inoffice == 1) {
//                echo 'already in office/school';
//            }
//            else {
//                writegameplayer($id,$inst,'inoffice',1);
                $ed = (int)readplayerprop($id, 'ed', $inst);
                $of = (int)readscale($inst, 4);
                $earning = 2*$of*$ed;
                
                //handling mimumwage
                $minwg = 2*$of + (int) readscale($inst, 15);
                $maxwg =2*$of*((int) readgamefund('highested', $inst))-(int) readscale($inst, 15);
                if($earning < $minwg){
                    $earning = $minwg;
                }
                else if($earning > $maxwg){
                    $earning = $maxwg;
                }
                
                //handling incometax
                $incometaxrate = (int) readscale($inst, 10);
                $taxamount = (int) readgamefund('taxamount', $inst);
                $incometax = round($earning*$incometaxrate/100,0);
                $earning -= $incometax;
                $taxamount += $incometax;
                
                //final handling
                $mu = (float)readplayerprop($id,'mu',$inst);
                writeplayerprop($id, $inst, 'mu', $mu+$earning);
                $sl = (int) readplayerprop($id, 'sl', $inst);
                writeplayerprop($id, $inst, 'sl', $sl+4);
                writegamefund('taxamount', $inst, $taxamount);
                echo $earning;
                $str = '['.$id.','.$earning.']';
//            }
        }
        else {
            echo "Value missing";
        }            
    }
    if($ac == 'o99'){
        if(isset($_REQUEST['value1'])) {
            $id = $_REQUEST['value1'];
            $str = '['.$id.']';
            echo 'true';
        }
    }    
    if($ac == 's01') {
        //visit school
        if(isset($_REQUEST['value1'])) { 
            $id = $_REQUEST['value1'];
            $str = '['.$id.']';
            $inhospital = readgameplayer($id, $inst, 'inhospital');
            if($inhospital > 0) {
                echo 'player is in hospital';
            }
            else {
                $inf = readplayerprop($id, 'inf', $inst);
                if($inf == 1){
                    writegamestation('school',$inst,'inf',1);
                }
                else {
                    $infprob = readgamestation('school',$inst,'inf');
                    writegamestation('school',$inst,'inf',0);
                    $infprob2 = (int) readgameplayer($id, $inst, 'inf');
                    writegameplayer($id, $inst, 'inf',$infprob+$infprob2);

                }
            echo 'true';
            }
        }
        else {
            echo "Value missing";
        }             
    }
    if($ac == "s02") {
        //increasing education
        if(isset($_REQUEST['value1'])) { 
            $id = $_REQUEST['value1'];
            
//            $inoffice = readgameplayer($id, $inst, 'inoffice');
//            if($inoffice == 1) {
//                echo 'already in office/school';
//            }
//            else {
                $cost = (int)readscale($inst, 5);
                $mu = (int)readplayerprop($id,'mu',$inst);
                $schoolfree = readscale($inst, 13);
                if($schoolfree == 0 && $cost> $mu) {
                    echo 'Insufficient money';
                }
                else {
//                    writegameplayer($id,$inst,'inoffice',1);
                    $ls = (float)readplayerprop($id, 'ls', $inst);
                    $ed = (float)readplayerprop($id,'ed',$inst);
                    $highested = (float) readgamefund('highested', $inst);
                    if(($ed+$ls)>$highested){
                        writegamefund('highested', $inst, $ed+$ls);
                    }
                    writeplayerprop($id, $inst, 'ed', $ed+$ls);
                    if($schoolfree == 0) {
                        writeplayerprop($id, $inst, 'mu', $mu-$cost);
                    }
                    else {
                        $taxamount = (int) readgamefund('taxamount', $inst);
                        writegamefund('taxamount', $inst, $taxamount - $cost);
                    }
                    $sl = (int) readplayerprop($id, 'sl', $inst);
                    writeplayerprop($id, $inst, 'sl', $sl+4);            
                    echo 'true';
                    $str = '['.$id.','.$ed+$ls.']';
                }
                
//            } 
        }
        else {
            echo "Value missing";
        }         
    }
    if($ac == 's99'){
        if(isset($_REQUEST['value1'])) {
            $id = $_REQUEST['value1'];
            $str = '['.$id.']';
            echo 'true';
        }
    }    
    if($ac == 'l01') {
        //visit bar
        $lockdown = readscale($inst, 14);
        if($lockdown == 0){
            if(isset($_REQUEST['value1'])) { 
                $id = $_REQUEST['value1'];
                $str = '['.$id.']';
                $inhospital = readgameplayer($id, $inst, 'inhospital');
                $inoffice = readgameplayer($id, $inst, 'inoffice');
                if($inhospital > 0) {
                    echo 'player is in hospital';
                }
                else {
                    if($inoffice > 0) {
                        echo 'player is already in lounge';
                    }
                    else {
                        $inf = (int)readplayerprop($id, 'inf', $inst);
                        if($inf == 1){
                            writegamestation('lounge',$inst,'inf','1');
                        }
                        else {
                            $infprob = readgamestation('school',$inst,'inf');
                            writegamestation('lounge',$inst,'inf',0);
                            $infprob2 = (int) readgameplayer($id, $inst, 'inf');
                            writegameplayer($id, $inst, 'inf',$infprob+$infprob2);
                        }
                        writegameplayer($id, $inst, 'inquarantine', 0);
                        echo 'true';
                    }
                }
            }
            else {
                echo "Value missing";
            }   
        }
        else{
            echo 'in lockdown';
        }
    }
    if($ac == "l02") {
        //relax
        if(isset($_REQUEST['value1'])) { 
            $id = $_REQUEST['value1'];
            $str = '['.$id.']';
            
            writegameplayer($id,$inst,'inoffice',1);
            $sl = (int)readplayerprop($id, 'sl', $inst);
            writeplayerprop($id, $inst, 'sl', $sl-6);
            echo 'true';
        }
        else {
            echo "Value missing";
        }          
    }
    if($ac == 'l99'){
        if(isset($_REQUEST['value1'])) {
            $id = $_REQUEST['value1'];
            $str = '['.$id.']';
            echo 'true';
        }
    } 
    if($ac == 'h01'){
        if(isset($_REQUEST['value1'])) {
            $id = $_REQUEST['value1'];
            $str = '['.$id.']';
            echo 'true';
        }
    }    
    if($ac == "h02") {
        //buy care package
        if(isset($_REQUEST['value1'])) { 
            $id = $_REQUEST['value1'];
            $str = '['.$id.']';
            writegameplayer($id,$inst,'inhospital',1);
            writegameplayer($id,$inst,'carepackage',1);
            $cost = readscale($inst, 20);
            $mu = readplayerprop($id, 'mu', $inst);
            writeplayerprop($id, $inst, 'mu', $mu-$cost);
            echo 'true';
        }
        else {
            echo "Value missing";
        }      
    }
    if($ac == 'h99'){
        if(isset($_REQUEST['value1'])) {
            $id = $_REQUEST['value1'];
            $str = '['.$id.']';
            echo 'true';
        }
    }    
    if($ac == "t01") {
            //visit the townhall
        if(isset($_REQUEST['value1'])) {
            $id = $_REQUEST['value1'];
            $str = $id;
            $inhospital = (int)readgameplayer($id, $inst, 'inhospital');
            if($inhospital > 0) {
                echo 'player is in hospital';
            }
            else {
                $inf = readplayerprop($id, 'inf', $inst);
                if($inf == 1){
                    writegamestation('townhall',$inst,'inf','1');
                }
                else {
                    $infprob = readgamestation('townhall',$inst,'inf');
                    writegamestation('townhall',$inst,'inf',0);
                    $infprob2 = (int) readgameplayer($id, $inst, 'inf');
                    writegameplayer($id, $inst, 'inf',$infprob+$infprob2);
                }
                writegameplayer($id, $inst, 'inquarantine', 0);
                echo 'true';
            }
        }
        else {
            echo "Value missing";
        }    
    }
    if($ac == 't99'){
        if(isset($_REQUEST['value1'])) {
            $id = $_REQUEST['value1'];
            $str = '['.$id.']';
            echo 'true';
        }
    }    
    if($ac == 'q01') {
        //stay at home
        if(isset($_REQUEST['value1'])) { 
            $id = $_REQUEST['value1'];
            $str = '['.$id.']';
            
            $sl = (int)readplayerprop($id, 'sl', $inst);
            writeplayerprop($id, $inst, 'sl', $sl-4);
            echo 'true';
        }
        else {
            echo "Value missing";
        }      
    }
    if($ac == 'q99'){
        if(isset($_REQUEST['value1'])) {
            $id = $_REQUEST['value1'];
            $str = '['.$id.']';
            echo 'true';
        }
    }    
    if($ac == 'reg') {
        // register
        if(isset($_REQUEST['value1']) && isset($_REQUEST['value2']) && isset($_REQUEST['value3'])) { 
            $id = $_REQUEST['value1'];
            $name = $_REQUEST['value2'];
            $pw = $_REQUEST['value3'];
            $str = '['.$id.','.$name.']';
            include 'register.php';
            echo register($id, $name, $inst,$pw);
        }
    }
    if($ac == 'init') {
        // game initialize
        include 'initialize.php';
        echo initialize($inst);
    }
    if($ac == 'ci') {
        // create instance
        include 'instancecreator.php';
        echo instancecreator($inst);
    }
    if($ac == 'li') {
        echo readinstances();
    }
    if($ac == 'nr'){
        include 'nextround.php';
        echo nextround($inst);        
    }
    if($ac == 'votedev') {
        $array = $_REQUEST['value1'];
        echo writevotedev($inst,$array);
    }
    if($ac == 'votetemp'){
        echo readvotetemp($inst);
    }
    if($ac == 'vote') {
        if(isset($_REQUEST['value1']) && isset($_REQUEST['value2']) && isset($_REQUEST['value3'])) {
            $id = $_REQUEST['value1'];
            $voteid = $_REQUEST['value2'];
            $vote = $_REQUEST['value3'];
            $str = '['.$id.','.$voteid.','.$vote.']';
            echo writevote($id,$inst,$voteid,$vote);
        }
        else {
            echo "Value missing";
        }
    }
    if($ac == 'closevote'){
        if(isset($_REQUEST['value1'])){
            $voteid = $_REQUEST['value1'];
            $str = '['.$voteid.']';
            echo closevote($inst,$voteid);
        }
        else {
            echo "Value missing";
        }            
    }
    if($ac == 'delvote') {
        if(isset($_REQUEST['value1'])){
            $voteid = $_REQUEST['value1'];
            $str = '['.$voteid.']';
            echo delvote($inst,$voteid);
        }
        else {
            echo "Value missing";
        }         
    }
    if($ac == 'openvote'){
        if(isset($_REQUEST['value1'])){
            $voteid = $_REQUEST['value1'];
            $str = '['.$voteid.']';
            echo openvote($inst,$voteid);
        }
        else {
            echo "Value missing";
        }            
    }
    if($ac == 'listopenvote') {
        if(isset($_REQUEST['value1'])){
            $voter = $_REQUEST['value1'];        
            echo listopenvote($inst,$voter);
        }
    }
    if($ac == 'ask'){
        //ask handling
        if(isset($_REQUEST['value1']) && isset($_REQUEST['value2'])) {        
            $id = $_REQUEST['value1'];
            $item = $_REQUEST['value2'];
            $str = '['.$id.','.$item.']';
            echo readplayerprop($id,$item,$inst);
        }
        else {
            echo "Value missing";
        } 
    }
    if($ac == 'askf') {
        //ask fund
        if(isset($_REQUEST['value1'])) {
            $item = $_REQUEST['value1'];
            $str = '['.$item.']';
            echo (int) readgamefund($item, $inst);
        }
    }
    if($ac == 'askgf') {
        $fund1 = (int) readgamefund('taxamount', $inst);
        $fund2 = (int) readgamefund('vac', $inst);
        $fund3 = (int) readgamefund('stocks', $inst);
        echo json_encode([$fund1,$fund2,$fund3]);
    }
    if($ac != 'li' && $ac != 'getlog' && $ac != 'getgame' && $ac != 'getplayer') {
        logwriter($ac, $str,$inst);
    }
    if($ac == 'getlog') {
        $handle = fopen('../instances/'.$inst.'/logs/globallog.txt','r');
        echo nl2br(fread($handle, filesize('../instances/'.$inst.'/logs/globallog.txt')));
        fclose($handle);  
    }
    
    if($ac == 'getgame') {
        echo readgamedatabase($inst);
    }
    if($ac == 'getplayer') {
        echo readplayerdatabase($inst);
    }    
    if($ac == 'sq02') {
        $url = explode('includes/ACHandler.php',$_SERVER['REQUEST_URI'])[0];
        echo $server.$url.'instances/'.$inst.'/news/news_glob.php';
    }
    if($ac == 'sq03') {
        $url = explode('includes/ACHandler.php',$_SERVER['REQUEST_URI'])[0];
        if(isset($_REQUEST['value1'])){
            $id = $_REQUEST['value1'];
            $blah = json_encode([$id,$inst]);
            echo $server.$url.'instances/'.$inst.'/news/news_ind.php?blah='.$blah;
        }
    }    
    if($ac == 'askhlt') {
        if(isset($_REQUEST['value1'])){
            $id = $_REQUEST['value1'];
            $hp = (int)readplayerprop($id,'hp',$inst);
            $sl = (int)readplayerprop($id,'sl',$inst);
            $inf = (int)readplayerprop($id,'inf',$inst);
            $geszustand = $inf*10 + $sl + 5;
            if($hp > 2*$geszustand) {
                echo 2;
            }
            else if($hp > $geszustand) {
                echo 1;
            }
            else {
                echo 0;
            }
        }
    }
    if($ac == 'getscale') {
        
        if(isset($_REQUEST['value1'])){
            $no = $_REQUEST['value1'];
            echo (int)readscale($inst,$no);
        }
    }
    
    if($ac == 'inhospital') {
        if(isset($_REQUEST['value1'])){
            $id = $_REQUEST['value1'];
            echo readgameplayer($id, $inst, 'inhospital');
        }
    }
    
    if($ac == 'getinfon') {
        $url = explode('includes/ACHandler.php',$_SERVER['REQUEST_URI'])[0];
        if(isset($_REQUEST['value1'])){
            $n = $_REQUEST['value1'];
            if(file_exists($server.$url.'instances/'.$inst.'/news/info'.$n.'.php')) {
                echo $server.$url.'instances/'.$inst.'/news/info'.$n.'.php';
            }
            else {
                echo "this info doesn't exist (yet)";
            }
        }        
    }
    
    if($ac == 'listpetition') {
        echo listvote($inst);
    }
    if($ac == 'pwcheck') {
        if(isset($_REQUEST['value1'])){
            $pw = $_REQUEST['value1'];
            if($pw == 'CoronaGame2022') {
                echo 'true';
            }
            else {
                echo 'false';
            }
        }
    }
    if($ac == 'banderole') {
        if(isset($_REQUEST['value1'])){
            $text = $_REQUEST['value1'];
            echo banderole($inst,$text);
        }
    }

}
