<?php
if (isset($_REQUEST['id'])) {
    $id = $_REQUEST['id'];
    $val = $_REQUEST['value'];
    $inst = $_REQUEST['inst'];
    $gen = $_REQUEST['gen'];
    $no = $_REQUEST['no'];
    echo $no;
    $xml=simplexml_load_file($inst."/pvq/language/spanish/questions_".$gen.".xml") or die("Error: Cannot create object");
    $player = $xml->item[(int)$no]->answer->addChild('player');
    $player->addChild('id',$id);
    $player->addChild('val',$val);
    file_put_contents($inst."/pvq/language/spanish/questions_".$gen.".xml",$xml->saveXML());
    include '../includes/writexml.php';
    include '../includes/readxml.php';
    $mu = (int)readplayerprop($id, 'mu', $inst);
    writeplayerprop($id, $inst, 'mu', $mu+1);
}
?>