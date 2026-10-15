using Xunit;

// integration tests share one mariadb instance, so run them serially
[assembly: CollectionBehavior(DisableTestParallelization = true)]
