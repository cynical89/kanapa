**Kanapa**

Simple async CouchDb client for DNX runtime.

Package availiable via feed https://www.myget.org/F/l0nley

![AppVeyor](https://ci.appveyor.com/api/projects/status/lu20j810qa1yk7v9?svg=true "AppVeyor")

**Example**

```csharp
      var dbName = "someDatabase";
      var client =  new CouchClient(new Uri(_hostName), new DefaultCouchMiddleware());
      await client.CreateDatabase(dbName)
      var entity = await client.Create(dbName, new SimpleObject
      {
        Name = name,
        Value = value
      });
      var document = await client.Get<SimpleObject>(dbName, entity.Id);
```

**License**

The MIT License (MIT)

Copyright (c) 2015 Uladzimir Harabtsou

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
