I've experienced a bug when using OData $apply. The point is that the response will be in the wrong format, not compliant with the OData protocol.

Some sample requests:

https://localhost:7109/odata/WeatherForecast?$apply=groupby((TemperatureC))

https://localhost:7109/odata/WeatherForecast?$apply=groupby((TemperatureC),aggregate($count%20as%20Count))

The responses:

![image](https://github.com/peterkovecses/ODataApplyDemo/assets/89272499/0794c46c-c762-4a77-9df2-82518a27273a)
![image](https://github.com/peterkovecses/ODataApplyDemo/assets/89272499/f179357e-a2c3-4b9a-a7fb-52299c112839)

As you can see, the @odata.context and value properties are missing, instead the value of the value property is returned directly.

I saw that the issue had already been reported, but I needed a quick solution.
I found that if the return value of the controller method is IEnumerable<T> or IQueriable<T> instead of IActionResult or ActionResult<T>, the response is in the correct format, but that was not an option for me, I had to find another solution. As a first step, I derived it from the EnableQueryAttribute class, and in the case of $apply, I added a marker key-value pair to the response header:

![image](https://github.com/peterkovecses/ODataApplyDemo/assets/89272499/c406984f-fe15-46e1-8925-76506a5c7fd0)

Next, I created a wrapper class:

![image](https://github.com/peterkovecses/ODataApplyDemo/assets/89272499/5ac154d3-955d-47a3-9ef3-004d3d4527ff)

I monitor the marker header in a middleware, and if it appears, I wrap the value in the response body so that it complies with the OData protocol.

![image](https://github.com/peterkovecses/ODataApplyDemo/assets/89272499/a9778261-e946-4600-a215-4959e89351d4)

In addition, I only had to register the middleware:

![image](https://github.com/peterkovecses/ODataApplyDemo/assets/89272499/0ffbc4d0-400d-4a58-92eb-dda1ad2bdc6a)

And the results:

![image](https://github.com/peterkovecses/ODataApplyDemo/assets/89272499/a1279e81-dc52-4281-a248-637267697378)
![image](https://github.com/peterkovecses/ODataApplyDemo/assets/89272499/da13139b-76f5-4a43-9ea6-d1f60243e744)


