
<html>
    <script>
        function backendexchange1(inst,ac) {
            var xhr = new XMLHttpRequest();
            xhr.onreadystatechange = function () {
                if (xhr.readyState == 4 && xhr.status == 200){   
                    alert(this.responseText);
                } 
                        }
            xhr.open('POST','includes/ACHandler.php',true);
            var fd = new FormData;
            fd.append('ac',ac);
            fd.append('inst',inst);
            xhr.send(fd);
        }
        function backendexchange2(inst,ac,value1) {
            var xhr = new XMLHttpRequest();
            xhr.onreadystatechange = function () {
                if (xhr.readyState == 4 && xhr.status == 200){   
                    alert(this.responseText);
                } 
                        }
            xhr.open('POST','includes/ACHandler.php',true);
            var fd = new FormData;
            fd.append('ac',ac);
            fd.append('inst',inst);
            fd.append('value1',value1);
            xhr.send(fd);
        }
         function backendexchange3(inst,ac,value1,value2) {
            var xhr = new XMLHttpRequest();
            xhr.onreadystatechange = function () {
                if (xhr.readyState == 4 && xhr.status == 200){   
                    alert(this.responseText);
                } 
            }
            xhr.open('POST','includes/ACHandler.php',true);
            var fd = new FormData;
            fd.append('ac',ac);
            fd.append('inst',inst);
            fd.append('value1',value1);
            
            fd.append('value2',value2);
            xhr.send(fd);
        }
          function backendexchange4(inst,ac,value1,value2,value3) {
            var xhr = new XMLHttpRequest();
            xhr.onreadystatechange = function () {
                if (xhr.readyState == 4 && xhr.status == 200){   
                    alert(this.responseText);
                } 
                        }
            xhr.open('POST','includes/ACHandler.php',true);
            var fd = new FormData;
            fd.append('ac',ac);
            fd.append('inst',inst);
            fd.append('value1',value1);
            fd.append('value2',value2);
            fd.append('value3',value3);
            xhr.send(fd);
        }          
    </script>
    <head>
        <meta charset="UTF-8">
        <title></title>
    </head>
    <body>
        <div style="height:100px;width:200px;background-color:grey;border: 1px solid black;border-radius:5px;display:block;float:left;margin:5px;" onclick="backendexchange2('test','m01','0000000000')"><p>m01</p></div>
        <div style="height:100px;width:200px;background-color:grey;border: 1px solid black;border-radius:5px;display:block;float:left;margin:5px;" onclick="backendexchange2('test','m03','0000000000')"><p>m03</p></div>
        <div style="height:100px;width:200px;background-color:grey;border: 1px solid black;border-radius:5px;display:block;float:left;margin:5px;" onclick="backendexchange2('test','m04','0000000000')"><p>m04</p></div>
        <div style="height:100px;width:200px;background-color:grey;border: 1px solid black;border-radius:5px;display:block;float:left;margin:5px;" onclick="backendexchange2('test','m05','0000000000')"><p>m05</p></div>
        <div style="height:100px;width:200px;background-color:grey;border: 1px solid black;border-radius:5px;display:block;float:left;margin:5px;" onclick="backendexchange3('test','m06','0000000000',10)"><p>m06</p></div>
        <div style="height:100px;width:200px;background-color:grey;border: 1px solid black;border-radius:5px;display:block;float:left;margin:5px;" onclick="backendexchange3('test','m07','0000000000',10)"><p>m07</p></div>
        <div style="height:100px;width:200px;background-color:grey;border: 1px solid black;border-radius:5px;display:block;float:left;margin:5px;" onclick="backendexchange4('test','m08','0000000000','0000000001',10)"><p>m08</p></div>
        <div style="height:100px;width:200px;background-color:grey;border: 1px solid black;border-radius:5px;display:block;float:left;margin:5px;" onclick="backendexchange2('test','o01','0000000000')"><p>o01</p></div>
        <div style="height:100px;width:200px;background-color:grey;border: 1px solid black;border-radius:5px;display:block;float:left;margin:5px;" onclick="backendexchange2('test','o02','0000000000')"><p>o02</p></div>
        <div style="height:100px;width:200px;background-color:grey;border: 1px solid black;border-radius:5px;display:block;float:left;margin:5px;" onclick="backendexchange2('test','s01','0000000000')"><p>s01</p></div>
        <div style="height:100px;width:200px;background-color:grey;border: 1px solid black;border-radius:5px;display:block;float:left;margin:5px;" onclick="backendexchange2('test','s02','0000000000')"><p>s02</p></div>
        <div style="height:100px;width:200px;background-color:grey;border: 1px solid black;border-radius:5px;display:block;float:left;margin:5px;" onclick="backendexchange2('test','l01','0000000000')"><p>l01</p></div>
        <div style="height:100px;width:200px;background-color:grey;border: 1px solid black;border-radius:5px;display:block;float:left;margin:5px;" onclick="backendexchange2('test','l02','0000000000')"><p>l02/p></div>
        <div style="height:100px;width:200px;background-color:grey;border: 1px solid black;border-radius:5px;display:block;float:left;margin:5px;" onclick="backendexchange1('test','votetemp')"><p>votetemp</p></div>
        <div style="height:100px;width:200px;background-color:grey;border: 1px solid black;border-radius:5px;display:block;float:left;margin:5px;" onclick="backendexchange3('test','vote','0000000000',1,'yes')"><p>vote</p></div>
        <div style="height:100px;width:200px;background-color:grey;border: 1px solid black;border-radius:5px;display:block;float:left;margin:5px;" onclick="backendexchange2('test','closevote',1)"><p>closevote</p></div>
        <div style="height:100px;width:200px;background-color:grey;border: 1px solid black;border-radius:5px;display:block;float:left;margin:5px;" onclick="backendexchange2('test','openvote',1)"><p>openvote</p></div>
        <div style="height:100px;width:200px;background-color:grey;border: 1px solid black;border-radius:5px;display:block;float:left;margin:5px;" onclick="backendexchange2('test','votedev','0000000000')"><p>votedev</p></div>
        <div style="height:100px;width:200px;background-color:grey;border: 1px solid black;border-radius:5px;display:block;float:left;margin:5px;" onclick="backendexchange4('test','reg','0000000000','Bernhard','01')"><p>reg1</p></div>
        <div style="height:100px;width:200px;background-color:grey;border: 1px solid black;border-radius:5px;display:block;float:left;margin:5px;" onclick="backendexchange4('test','reg','0000000001','Josef','02')"><p>reg2</p></div>
        <div style="height:100px;width:200px;background-color:grey;border: 1px solid black;border-radius:5px;display:block;float:left;margin:5px;" onclick="backendexchange1('test','init')"><p>init</p></div>
        <div style="height:100px;width:200px;background-color:grey;border: 1px solid black;border-radius:5px;display:block;float:left;margin:5px;" onclick="backendexchange3('test','ask','0000000000','mu')"><p>ask</p></div>
        <div style="height:100px;width:200px;background-color:grey;border: 1px solid black;border-radius:5px;display:block;float:left;margin:5px;" onclick="backendexchange2('test','askf','vac')"><p>askf</p></div>
        <div style="height:100px;width:200px;background-color:grey;border: 1px solid black;border-radius:5px;display:block;float:left;margin:5px;" onclick="backendexchange1('test','nr')"><p>nr</p></div>
        <div style="height:100px;width:200px;background-color:grey;border: 1px solid black;border-radius:5px;display:block;float:left;margin:5px;" onclick="backendexchange2('test','ci','test')"><p>ci</p></div>
        <div style="height:100px;width:200px;background-color:grey;border: 1px solid black;border-radius:5px;display:block;float:left;margin:5px;" onclick="backendexchange1('test','li')"><p>li</p></div>
        <div style="height:100px;width:200px;background-color:grey;border: 1px solid black;border-radius:5px;display:block;float:left;margin:5px;" onclick="backendexchange2('test','getscale','0')"><p>getscale</p></div>
    </body>
</html>
