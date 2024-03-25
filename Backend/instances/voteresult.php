<?php
include '../../plotmaker.php';
?>
<html>
    <head>
        <script src="../../plotly-2.0.0.min.js"></script>
        <script type="text/javascript" >
            function plotmaker(array2,str1) {
                var array = JSON.parse(array2);
                var graphcontainer = document.getElementById('graphcontainer');
                var rundenarray = [];
                for(i=0;i<array.length;i++){
                    rundenarray.push(i+1);
                }
                var infectiontrace = {x:['sí','no'], y: array,type: 'bar'};

                var layout = {
                    title: str1,
                    showlegend: true
                };
                Plotly.newPlot(graphcontainer, [infectiontrace],layout);

            }

        </script> 
        
        
    </head>
    <body>
        <div id="graphcontainer">
            
        </div>
        <script>
            zjznudfnsrzabezntum
        </script>
    </body>       
</html>

