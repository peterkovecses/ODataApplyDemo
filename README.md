I've experienced a bug when using OData $apply. The point is that the response will be in the wrong format, not compliant with the OData protocol.

Some sample requests:
https://localhost:7109/odata/WeatherForecast?$apply=groupby((TemperatureC))
https://localhost:7109/odata/WeatherForecast?$apply=groupby((TemperatureC),aggregate($count%20as%20Count))

The responses:
![image](https://github.com/peterkovecses/ODataApplyDemo/assets/89272499/0794c46c-c762-4a77-9df2-82518a27273a)
![image](https://github.com/peterkovecses/ODataApplyDemo/assets/89272499/f179357e-a2c3-4b9a-a7fb-52299c112839)

As you can see, the @odata.context and value properties are missing, instead the value of the value property is returned directly.

I saw that the issue had already been reported, but I needed a quick solution. 
As a first step, I derived it from the EnableQueryAttribute class, and in the case of $apply, I added a marker key-value pair to the response header:
![image](https://github.com/peterkovecses/ODataApplyDemo/assets/89272499/4f63fb33-3df8-4e2b-a1de-86a094fd12b0)

Next, I created a wrapper class:
![image](https://github.com/peterkovecses/ODataApplyDemo/assets/89272499/5ac154d3-955d-47a3-9ef3-004d3d4527ff)

I monitor the marker header in a middleware, and if it appears, I wrap the value in the response body so that it complies with the OData protocol.
![image](https://github.com/peterkovecses/ODataApplyDemo/assets/89272499/a9778261-e946-4600-a215-4959e89351d4)

In addition, I only had to register the middleware:
app.UseMiddleware<ODataApplyPatchMiddleware>();

And the results:
![image](https://github.com/peterkovecses/ODataApplyDemo/assets/89272499/a1279e81-dc52-4281-a248-637267697378)
![image](https://github.com/peterkovecses/ODataApplyDemo/assets/89272499/da13139b-76f5-4a43-9ea6-d1f60243e744)


