<?php

if (isset($_REQUEST['blah'])) {
    $array = json_decode($_REQUEST['blah']);
    session_start();
    $_SESSION['id'] = $array[0];
    $_SESSION['gen'] = $array[1];
    $_SESSION['inst'] = $array[2];
    session_write_close();
}

function readsurveyxml($gen,$id,$inst){
    $xml=simplexml_load_file($inst."/pvq/language/english/questions_".$gen.".xml") or die("Error: Cannot create object");
    $true = true;
    $i = -1;
    while($true) {
        $i += 1;
        if(isset($xml->item[$i])) {
            if(isset($xml->item[$i]->answer->player)) {
                $true2 = false;
                foreach($xml->item[$i]->answer->player as $player) {
                    if($player->id == $id){
                        $true2 = true;
                    }
                }
                $true = $true2;
            }
            else{
                $true = false;
            }
        }
        else {
            return 'Todas las preguntas fueron respondidas';
        }
        
    }
    $_SESSION['no'] = $i;
    return (string)$xml->item[$i]->content;  
}
?>

<html>
    <script>
        function surveyhandler(no) {
            var array = [];
            array.push(document.getElementById('a1'));
            array.push(document.getElementById('a2'));
            array.push(document.getElementById('a3'));
            array.push(document.getElementById('a4'));
            array.push(document.getElementById('a5'));
            array.push(document.getElementById('a6'));
            array.push(document.getElementById('a7'));
            var val = 0;
            for(i=1;i<7;i++){
                if(array[i].checked) {
                    val = array[i].value;
                }
            }
            var xhr = new XMLHttpRequest();
            xhr.onreadystatechange = function () {
                if (xhr.readyState == 4 && xhr.status == 200){   
                    location.reload();
                } 
            }
            xhr.open('POST','surveyhandler.php',true);
            var fd = new FormData;
            fd.append('id','<?php session_start(); echo $_SESSION['id']; ?>');
            fd.append('value',val);
            fd.append('inst','<?php echo $_SESSION['inst']; ?>');
            fd.append('no',no);
            fd.append('gen','<?php echo $_SESSION['gen']; ?>');
            xhr.send(fd);            
        
        }
    </script>
    <body>
       
        <form>
            <p><i><?php echo readsurveyxml($_SESSION['gen'], $_SESSION['id'], $_SESSION['inst'])?></i></p>
            <div style="display:<?php $r = readsurveyxml($_SESSION['gen'], $_SESSION['id'],$_SESSION['inst']);if($r == 'Todas las preguntas fueron respondidas'){echo 'none';}else{echo 'block';}?>">
            <p>¿En qué medida se parece esta persona a ti? Por favor, envíe su respuesta</p>
            <input type="radio" id="a1" name="answer" value="1">
            <label for="a1">muy parecida</label>
            <input type="radio" id="a2" name="answer" value="2">
            <label for="a2">parecida</label>
            <input type="radio" id="a3" name="answer" value="3">
            <label for="a3">más bien parecida</label>
            <input type="radio" id="a4" name="answer" value="4">
            <label for="a4">más bien diferente</label>
            <input type="radio" id="a5" name="answer" value="5">
            <label for="a5">diferente</label>   
            <input type="radio" id="a6" name="answer" value="6">
            <label for="a6">muy diferente</label>             
            <input type="radio" id="a7" name="answer" value="-1">
            <label for="a7">sin respuesta</label><br><br>
            <input type="submit" value="Submit" onclick="surveyhandler(<?php echo $_SESSION['no']; ?>);return false;">
            </div>
        </form>
        
    </body>
    
</html>