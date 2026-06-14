// SQLCipher temp-file databases + Microsoft.Data.Sqlite connection pooling can race across
// parallel test classes (intermittent "file in use" on cleanup). Serialize to keep the suite
// deterministic; the suite is fast enough that this is a non-issue.
[assembly: Xunit.CollectionBehavior(DisableTestParallelization = true)]
