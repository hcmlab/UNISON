<?php

function logwriter($ac,$str,$inst) {
    $heute = getdate();
    if($heute['minutes']<10){
        $datum = $heute['mday'].$heute['month'].$heute['year'].$heute['hours']."0".$heute['minutes'];
    }
    else {
        $datum = $heute['mday'].$heute['month'].$heute['year'].$heute['hours'].$heute['minutes'];
    }
    $handle = fopen('../instances/'.$inst.'/logs/globallog.txt','a');
    $str2 = $datum.' '.$ac.' '.$str."\n\r";
    fwrite($handle,$str2);
    fclose($handle);
}

function banderole($inst,$text) {
    $handle = fopen('../instances/'.$inst.'/news/banderole.txt','w');
    fwrite($handle,$text);
    fclose($handle);
    return 'true';
}