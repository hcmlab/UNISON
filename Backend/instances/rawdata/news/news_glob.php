<?php
?>
<html>
    <head>
        <link rel="stylesheet" type="text/css" href="../../style1.css" />
        <script>
            var i=1;
            function changeiframe() {
                var iframe = document.getElementById('iframe');
                i+= 1;
                var xhr = new XMLHttpRequest();
                xhr.open('HEAD','info'+i+'.php',false)
                xhr.send();
                if(xhr.status == 200) {
                    iframe.src = 'info'+i+'.php';
                }
                else {
                    i = 1;
                    iframe.src = 'info1.php';
                }
                
            }
            var k = true;
            function changepicture() {
                if(k){
                    k = false;
                    document.getElementById('gif1').style.display = 'none';
                    document.getElementById('gif2').style.display = 'block';
                }
                else {
                    document.getElementById('gif2').style.display = 'none';
                    document.getElementById('gif1').style.display = 'block';
                }
            }
        </script>
    </head>
<body style="overflow:hidden;">
    <div id="containercontainer"> 
        <image id="gif1" src="../../ai-fora.png" style="float:left">
        <iframe id="gif2" src="inhospital.txt" style="display:none;width:474px;height:644px;"></iframe>
        <iframe class="plotcontainer" id="iframe" src="info1.php"></iframe>
    </div>
    <iframe id="banderole" src="banderole.txt"></iframe>
   <script>
       setInterval(function () {changeiframe();},40000);
       setInterval(function () {changepicture();},20000);
       var m = 0;
       var interval2 = setInterval(function () {if(m<1300){document.getElementById('banderole').style.transform = 'translate(-'+m+'px,0)';m+=1;}else{m=0;}},20);
   </script> 
</body>
</html>