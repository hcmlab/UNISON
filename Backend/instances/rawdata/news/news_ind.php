<?php 
if (isset($_REQUEST['blah'])) {
    $array = json_decode($_REQUEST['blah']);
    session_start();
    $_SESSION['id'] = $array[0];
    $_SESSION['inst'] = $array[1];
    
    session_write_close();
}

function readplayerprop($id,$prop){
    $xml=simplexml_load_file("../data/playerdata.xml") or die("Error: Cannot create object");
    foreach($xml->player as $player) {
        if($player->rfid == $id) {
            return $player->$prop;
        }
    }
}
function readgameplayer($id,$item){
    $xml=simplexml_load_file("../data/gamedata.xml") or die("Error: Cannot create object");
    $k = $xml->count();
    foreach($xml->round[$k-1]->player as $player) {
        if($player->rfid == $id) {
            return $player->$item;
        }
    }    
}

?>
<html>
    <head>
        <link rel="stylesheet" type="text/css" href="../../style1.css" />
        <script>
            var i=1;
            function changeiframe(src) {
                var iframe = document.getElementById('iframe');
                iframe.style.display = 'block';
                iframe.src = src;
                var div = document.getElementById('personalinformation');
                div.style.display = 'none';
                div.innerHTML = '';
            }
            
            function createbuttons() {
                var btc = document.getElementById('buttoncontainer');
                var p2 = document.createElement('P');
                p2.innerHTML = 'General information about the state of the game:';
                p2.setAttribute('class','buttonheadline');
                btc.appendChild(p2);
                var p1 = document.createElement('P');
                for(j=0;j<20;j++){
                    var xhr = new XMLHttpRequest();
                    xhr.open('HEAD','info'+j+'.php',false)
                    xhr.send();
                    if(xhr.status == 200) {
                        var bt = document.createElement('INPUT');
                        bt.setAttribute('type','button');
                        bt.setAttribute('class','button');
                        bt.setAttribute('onclick','changeiframe("info'+j+'.php")');
                        bt.value = 'Information '+j;
                        p1.appendChild(bt);
                    }
                }
                var bt = document.createElement('INPUT');
                bt.setAttribute('type','button');
                bt.setAttribute('class','button');
                bt.setAttribute('onclick','changeiframe("inhospital.txt")');
                bt.value = 'in hospital';
                p1.appendChild(bt);
                btc.appendChild(p1);
                var p = document.createElement('P');            
            }
            
            
            function displayinfo2(rfid) {
                var iframe = document.getElementById('iframe');
                iframe.style.display = 'none';
                var div = document.getElementById('personalinformation');
                div.style.display = 'block';
                div.innerHTML = '';
                var p1 = document.createElement('P');
                p1.setAttribute('class','personalinformation');
                p1.innerHTML = 'Personal information of <?php echo readplayerprop($_SESSION['id'], 'name');?>';
                var p2 = document.createElement('P');
                p2.setAttribute('class','personalinformation');
                p2.innerHTML = 'Current money account: <?php echo readplayerprop($_SESSION['id'], 'mu');?>';
                var p3 = document.createElement('P');
                p3.setAttribute('class','personalinformation');
                var geszust = parseInt(<?php echo readplayerprop($_SESSION['id'], 'inf');?>)*10+parseInt(<?php echo readplayerprop($_SESSION['id'], 'sl');?>)+5;
                var ges = parseInt(<?php echo readplayerprop($_SESSION['id'], 'hp');?>);
                var str = '';
                if(ges > 2*geszust){
                    str = 'good';
                }
                else if(ges > geszust){
                    str = 'problematic';
                }
                else {
                    str = 'critical';
                }
                p3.innerHTML = 'Current health status: '+str;

                var boughttest = parseInt(<?php echo readgameplayer($_SESSION['id'], 'boughttest');?>);
                var inhospital = parseInt(<?php echo readgameplayer($_SESSION['id'], 'inhospital');?>);
                if(inhospital > 0) {
                    var p4 = document.createElement('P');
                    p4.setAttribute('class','personalinformation');
                    p4.innerHTML = 'You are currently in the hospital.';
                }
                            
                var ph1 = document.createElement('P');
                ph1.setAttribute('class','personalinformation');
                var ph2 = document.createElement('P');
                ph2.setAttribute('class','personalinformation');
                var ph3 = document.createElement('P');
                ph3.setAttribute('class','personalinformation');
                if(boughttest == 1){
                    ph1.innerHTML = 'Current health points: <?php echo readplayerprop($_SESSION['id'], 'hp');?>';
                    ph2.innerHTML = 'Current stress level: <?php echo readplayerprop($_SESSION['id'], 'sl');?>';
                    if(parseInt(<?php echo readplayerprop($_SESSION['id'], 'inf');?>) == 1) {
                        ph3.innerHTML = 'You are currently infected.';
                    }
                    else {
                        ph3.innerHTML = 'You are currently not infected.';
                    }
                }
                var bt = document.createElement('INPUT');
                bt.setAttribute('type','button');
                bt.setAttribute('class','button');
                bt.value = 'Cerrar info';
                bt.setAttribute('onclick','changeiframe("info1.php")');

                div.appendChild(p1);
                div.appendChild(p2);
                div.appendChild(p3);
                div.appendChild(p4);
                div.appendChild(ph1);
                div.appendChild(ph2);
                div.appendChild(ph3);
                div.appendChild(bt);
                if(inhospital > 1) {
                    var bt2 = document.createElement('INPUT');
                    bt2.setAttribute('type','button');
                    bt2.setAttribute('class','button');
                    bt2.value = 'Compra el paquete de atención.';
                    bt2.setAttribute('onclick','buycarepackage("'+rfid+'")');
                    div.appendChild(bt2);
                }
            }

            
            function buycarepackage(rfid) {
                if(confirm('Confirmar')) {
                    var url = document.location.href;
                    url = url.replace('http://localhost/Coronagameproject/instances/','');
                    var inst = url.replace('/news/news_ind.php','');
                    var xhr = new XMLHttpRequest();
                    xhr.onreadystatechange = function () {
                        if (xhr.readyState == 4 && xhr.status == 200){   
                            alert(this.responseText);
                        } 
                    }
                    xhr.open('POST','http://localhost/Coronagameproject/includes/ACHandler.php',true);
                    var fd = new FormData;
                    fd.append('ac','h02');
                    fd.append('inst',inst);
                    fd.append('value1',rfid);
                    xhr.send(fd);
                }
                
            }
            function survey (){
                document.getElementById('dialogprompt').style.display = 'block';
            }
            function survey2 () {
                document.getElementById('dialogprompt').style.display = 'none';
                var id = '<?php echo $_SESSION['id'];?>';
                var gen = document.getElementById('genderselect').value;
                var iframe = document.getElementById('iframe');
                var inst = '<?php echo $_SESSION['inst'];?>';
                var blah = JSON.stringify([id,gen,inst]);
                iframe.src = '../../surveyform_english.php?blah='+blah;
            }

        </script>
    </head>
<body style="overflow:hidden;">
    <div id="dialogprompt" style="display:none;z-index:100;height:100%;width:100%;background-color:lightgrey;border-radius:5px;justify-content: center;"><div style="width:30%;padding-top:10%;padding-left:40%"><p>Please choose your gender.</p><p><select id="genderselect"><option value="male">male</option><option value="female">female</option><option value="diverse">other</option></select></p><p><input id="surveybutton" value="Send" type=button" onclick="survey2();return false;"></p></div></div>
    <div id="containercontainer2"> 
        <iframe class="plotcontainer" id="iframe" src="info1.php"></iframe>
        <div id="personalinformation" style="display:none"></div>
    </div>
    <div id="buttoncontainer" class="buttoncontainer">
        <p class="buttonheadline">Get personal information:</p>
        <p>
            <input type="button" class="button" value="Personal info" onclick="displayinfo2('<?php echo $_SESSION['id']; ?>')">
            <input type="button" class="button" value="Survey" onclick="survey('<?php echo $_SESSION['id']; ?>')">
        </p>
        <p class="buttonheadline">Get general information about the game:</p>
        <p>
            <input type="button" class="button" value="Attributes - ED" onclick="changeiframe('../../playerwiki/language/english/attributes/ed.txt')">
            <input type="button" class="button" value="Attributes - HP" onclick="changeiframe('../../playerwiki/language/english/attributes/hp.txt')">
            <input type="button" class="button" value="Attributes - IS" onclick="changeiframe('../../playerwiki/language/english/attributes/is.txt')">
            <input type="button" class="button" value="Attributes - MU" onclick="changeiframe('../../playerwiki/language/english/attributes/mu.txt')">
            <input type="button" class="button" value="Attributes - SL" onclick="changeiframe('../../playerwiki/language/english/attributes/sl.txt')">
            <input type="button" class="button" value="Petitions" onclick="changeiframe('../../playerwiki/language/english/votes/general.txt')">
            <input type="button" class="button" value="Disappropriation" onclick="changeiframe('../../playerwiki/language/english/votes/disappropriation.txt')">
            <input type="button" class="button" value="Insurance" onclick="changeiframe('../../playerwiki/language/english/votes/insurance.txt')">
            <input type="button" class="button" value="Lockdown" onclick="changeiframe('../../playerwiki/language/english/votes/lockdown.txt')">
            <input type="button" class="button" value="Minimum wage" onclick="changeiframe('../../playerwiki/language/english/votes/minwage.txt')">
            <input type="button" class="button" value="School free" onclick="changeiframe('../../playerwiki/language/english/votes/schol.txt')">
            <input type="button" class="button" value="Social safety" onclick="changeiframe('../../playerwiki/language/english/votes/socialsafety.txt')">
            <input type="button" class="button" value="Taxes" onclick="changeiframe('../../playerwiki/language/english/votes/tax.txt')">
            <input type="button" class="button" value="Home" onclick="changeiframe('../../playerwiki/language/english/station/home.txt')">
            <input type="button" class="button" value="Lounge" onclick="changeiframe('../../playerwiki/language/english/station/lounge.txt')">
            <input type="button" class="button" value="Mall" onclick="changeiframe('../../playerwiki/language/english/station/mall.txt')">
            <input type="button" class="button" value="Office" onclick="changeiframe('../../playerwiki/language/english/station/office.txt')">
            <input type="button" class="button" value="School" onclick="changeiframe('../../playerwiki/language/english/station/school.txt')">
            <input type="button" class="button" value="Townhall" onclick="changeiframe('../../playerwiki/language/english/station/townhall.txt')">
        </p>
        <script>
            createbuttons();
        </script>
    </div>
</body>
</html>