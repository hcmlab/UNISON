<?php
include '../../plotmaker.php';
?>
<html>
    <head>
        <script src="../../plotly-2.0.0.min.js"></script>
        <script type="text/javascript" >
            function plotmaker(array2,str1,str2,str3) {
                var array = JSON.parse(array2);
                var graphcontainer = document.getElementById('graphcontainer');
                if(array.length < 2) {
                    graphcontainer.innerHTML = 'The current investment into the vaccine fund is '.array[0];
                }
                else{
                
                    var rundenarray = [];
                    for(i=0;i<array.length;i++){
                        rundenarray.push(i+1);
                    }
                    var infectiontrace = {x:rundenarray, y: array,type: 'scatter'};
                    
                    var layout = {
                        title: str1,
                        xaxis: {
                          range: [1,Math.max(...rundenarray)],
                          autorange: false,                    
                          title: str2
                        },
                        yaxis: {
                          range: [0,Math.max(...array)],
                          autorange: false,                    
                          title: str3
                        }
                    };
                    Plotly.newPlot(graphcontainer, [infectiontrace],layout);
                }
            }

        </script> 
        
        
    </head>
    <body>
        <div id="graphcontainer">
            
        </div>
        <script>
            plotmaker('<?php echo plotmaker('vaccine'); ?>','Current investment in vaccine fund','Status in round','Investment in MU');
        </script>
    </body>       
</html>

