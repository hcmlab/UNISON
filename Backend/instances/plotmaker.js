include('plotly-2.0.0.min.js');

function plotmaker(array,str1,str2,str3) {
    var rundenarray = [];
    for(i=0;i<array.length;i++){
        rundenarray.push(i+1);
    }
    var infectiontrace = {x:rundenarray, y: array,type: 'scatter'};
    var graphcontainer = document.getElementById('graphcontainer');
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

