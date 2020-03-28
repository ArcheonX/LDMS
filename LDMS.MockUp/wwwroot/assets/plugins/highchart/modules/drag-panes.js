/*
 Highstock JS v7.2.1 (2019-10-31)

 Drag-panes module

 (c) 2010-2019 Highsoft AS
 Author: Kacper Madej

 License: www.highcharts.com/license
*/
(function(a){"object"===typeof module&&module.exports?(a["default"]=a,module.exports=a):"function"===typeof define&&define.amd?define("highcharts/modules/drag-panes",["highcharts","highcharts/modules/stock"],function(b){a(b);a.Highcharts=b;return a}):a("undefined"!==typeof Highcharts?Highcharts:void 0)})(function(a){function b(a,r,b,t){a.hasOwnProperty(r)||(a[r]=t.apply(null,b))}a=a?a._modules:{};b(a,"modules/drag-panes.src.js",[a["parts/Globals.js"],a["parts/Utilities.js"]],function(a,b){var r=b.isNumber,
t=b.objectEach,z=a.hasTouch;b=a.merge;var w=a.wrap,e=a.addEvent,x=a.relativeLength,l=a.Axis,y=a.Pointer;b(!0,l.prototype.defaultYAxisOptions,{minLength:"10%",maxLength:"100%",resize:{controlledAxis:{next:[],prev:[]},enabled:!1,cursor:"ns-resize",lineColor:"#cccccc",lineDashStyle:"Solid",lineWidth:4,x:0,y:0}});a.AxisResizer=function(c){this.init(c)};a.AxisResizer.prototype={init:function(c,a){this.axis=c;this.options=c.options.resize;this.render();a||this.addMouseEvents()},render:function(){var c=
this.axis,a=c.chart,d=this.options,b=d.x,e=d.y,h=Math.min(Math.max(c.top+c.height+e,a.plotTop),a.plotTop+a.plotHeight),k={};a.styledMode||(k={cursor:d.cursor,stroke:d.lineColor,"stroke-width":d.lineWidth,dashstyle:d.lineDashStyle});this.lastPos=h-e;this.controlLine||(this.controlLine=a.renderer.path().addClass("highcharts-axis-resizer"));this.controlLine.add(c.axisGroup);d=a.styledMode?this.controlLine.strokeWidth():d.lineWidth;k.d=a.renderer.crispLine(["M",c.left+b,h,"L",c.left+c.width+b,h],d);this.controlLine.attr(k)},
addMouseEvents:function(){var c=this,a=c.controlLine.element,d=c.axis.chart.container,b=[],v,h,k;c.mouseMoveHandler=v=function(a){c.onMouseMove(a)};c.mouseUpHandler=h=function(a){c.onMouseUp(a)};c.mouseDownHandler=k=function(a){c.onMouseDown(a)};b.push(e(d,"mousemove",v),e(d.ownerDocument,"mouseup",h),e(a,"mousedown",k));z&&b.push(e(d,"touchmove",v),e(d.ownerDocument,"touchend",h),e(a,"touchstart",k));c.eventsToUnbind=b},onMouseMove:function(c){c.touches&&0===c.touches[0].pageX||!this.grabbed||(this.hasDragged=
!0,this.updateAxes(this.axis.chart.pointer.normalize(c).chartY-this.options.y))},onMouseUp:function(c){this.hasDragged&&this.updateAxes(this.axis.chart.pointer.normalize(c).chartY-this.options.y);this.grabbed=this.hasDragged=this.axis.chart.activeResizer=null},onMouseDown:function(c){this.axis.chart.pointer.reset(!1,0);this.grabbed=this.axis.chart.activeResizer=!0},updateAxes:function(c){var a=this,d=a.axis.chart,b=a.options.controlledAxis,e=0===b.next.length?[d.yAxis.indexOf(a.axis)+1]:b.next;b=
[a.axis].concat(b.prev);var h=[],k=!1,p=d.plotTop,m=d.plotHeight,l=p+m;c=Math.max(Math.min(c,l),p);var q=c-a.lastPos;1>q*q||([b,e].forEach(function(b,e){b.forEach(function(b,g){var f=(b=r(b)?d.yAxis[b]:e||g?d.get(b):b)&&b.options;if(f&&"navigator-y-axis"!==f.id){g=b.top;var u=Math.round(x(f.minLength,m));var n=Math.round(x(f.maxLength,m));e?(q=c-a.lastPos,f=Math.round(Math.min(Math.max(b.len-q,u),n)),g=b.top+q,g+f>l&&(n=l-f-g,c+=n,g+=n),g<p&&(g=p,g+f>l&&(f=m)),f===u&&(k=!0),h.push({axis:b,options:{top:100*
(g-p)/m+"%",height:100*f/m+"%"}})):(f=Math.round(Math.min(Math.max(c-g,u),n)),f===n&&(k=!0),c=g+f,h.push({axis:b,options:{height:100*f/m+"%"}}))}})}),k||(h.forEach(function(a){a.axis.update(a.options,!1)}),d.redraw(!1)))},destroy:function(){var a=this;delete a.axis.resizer;this.eventsToUnbind&&this.eventsToUnbind.forEach(function(a){a()});a.controlLine.destroy();t(a,function(c,b){a[b]=null})}};l.prototype.keepProps.push("resizer");e(l,"afterRender",function(){var c=this.resizer,b=this.options.resize;
b&&(b=!1!==b.enabled,c?b?c.init(this,!0):c.destroy():b&&(this.resizer=new a.AxisResizer(this)))});e(l,"destroy",function(a){!a.keepEvents&&this.resizer&&this.resizer.destroy()});w(y.prototype,"runPointActions",function(a){this.chart.activeResizer||a.apply(this,Array.prototype.slice.call(arguments,1))});w(y.prototype,"drag",function(a){this.chart.activeResizer||a.apply(this,Array.prototype.slice.call(arguments,1))})});b(a,"masters/modules/drag-panes.src.js",[],function(){})});
//# sourceMappingURL=drag-panes.js.map