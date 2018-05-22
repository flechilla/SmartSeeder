
# SmartSeeder

**SmartSeeder** aims to accomplish an easy way to apply seeds for ASP.NET CORE projects.

## Implementing the Seeds

For the implementation of your app's seeds, create a class for each type of entity that you need to seed. For this, implement the interface `ISeed<TDbContext>`. 

#### This interface contains two objects:

* `void AddOrUpdate(TDbContext context, int amountOfObjects = 20)` : 

This method must contain the logic for the creation of the objects, or at least must call the method(s) with this logic. 

The amount of object to be added is declared in the `param` **amountOfObjects**. By default, its value is 20.

After finishing of adding the objects to the specific `DbSet`, invoke the method `context.SaveChanges` in order to save the changes to the DB.

* `int OrderToByApplied { get; }`:

Must of the time, we have to seed objects that are related to other. So, in order to add these new objects, the parents must exist in the DB. The use of this property is to declare the order in which the seed must be applied.

## Invoking the seeds

To invoke the seeds, in the method `Configure` of the `StartUp`, call the method `EnsureSeedData<TDbContext>`. This is an extension method of the interface `IApplicationBuilder`, so we must invoke this method from the param object of this type in the `Configure` method. 

The param type `TDbContext` is the type of the context used to run the seeds.

---

## Example of use

### Seed class

``` C#
...
using NLipsum.Core;
using SeedEngine.Core;

namespace RandomThoughts.DataAccess.Seeds
{
    /// <inheritdoc />
    /// Contains the implementation of the
    /// <see cref="ThoughtHole" /> objects
    public class ThoughtHoleSeeds : ISeed<RandomThoughtsDbContext>
    {
        ///<inheritdoc />
        public int OrderToByApplied => 2;

        public void AddOrUpdate(RandomThoughtsDbContext context, int amountOfObjects = 20)
        {
            // Check if the DB already have objects of this type
            if (context.ThoughtHoles.Any())
                return;

            var thoughtHoles = new List<ThoughtHole>(amountOfObjects);
            var lipsumGen = new LipsumGenerator();

            // Iterates amountOfObjects times and create the desired objects
            for (var i = 0; i < amountOfObjects; i++)
            {
                var newHole = new ThoughtHole
                {
                    Name = lipsumGen.GenerateSentences(1)[0],
                    Description = lipsumGen.GenerateLipsum(40),
                    Likes = new Random(DateTime.UtcNow.Millisecond).Next(5, 1000),
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow,
                    Views = new Random(DateTime.UtcNow.Millisecond).Next(5, 1000000)
                };

                thoughtHoles.Add(newHole);
            }

            //Add the objects to the `DbSet`
            context.ThoughtHoles.AddRange(thoughtHoles);
            //Save the new objects to the DB if needed
            context.SaveChanges();
        }
    }
}
```

In the above example, we are seeding the table `ThoughtHoles` (`ThoughtHole` entity). This is the 2nd type of object to be seeded, so we assign the value 2 to the property `OrderToByApplied`.

* Asks if the DB already contains objects of this type, if it does, then the seeds are not needed. 
* Creates the list `thoughtHoles` to minimize the number of operations that `EF` has to perform ( won't track the objects). 
* Iterates `amountOfObjects` times and creates the objects and add them to the list. 
* Adds the objects to the `DbSet` using the `AddRange` method ( try to always use this method when adding a set of objects ;)-). 
* Because we need this objects in the DB as soon as possible, we invoke the `SaveChanges` method. 

### Invoking the seeds

```C#
namespace RandomThoughts
{
    public class Startup
    {
        ...

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
                app.UseDatabaseErrorPage();
                
                //apply all the seeds
                app.EnsureSeedData<RandomThoughtsDbContext>();
            }
            
            ...
        }
    }
}
        
```

Apply the seeds by invoking the method `EnsureSeedData`, asking first for the current environment. Because we just want to apply the seeds in the *development* environment, we invoke the method inside the conditional block. 


## Constraints

1. The seeds classes must be implemented in the same assembly ( i.e in the same project) where the application contains the implementation of the `DbContext` used to seed the DB.
2. The seeds classes must implement the interface `ISeed<TDbContext>`.
3. In the seed classes, the logic of the seeds must be implemented in the method `AddOrUpdate`, or at least, be called from this method.
4. Must invoke the extension method `EnsureSeedData` in the `StartUp.Configure` method.

## Conclusions

With the use of the **SmartSeeder** library is possible to have a seed class per each object that we want to seed and invoking these seeds automatically ( using reflection).

Besides this library is functional and accomplishes what is intended to, I believe there are many improvements to be implemented. This is a work in progress, if you want to collaborate, please fork the project and do the work!!! 

If you find a bug or think about improvements, please use the **Issues** to report them.

I hope you enjoy the library!!! 
