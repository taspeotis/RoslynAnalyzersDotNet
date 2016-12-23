# RoslynAnalyzersDotNet

Miscellaneous code analysis.

## License

*RoslynAnalyzersDotNet* is licensed under the [*MIT License*](LICENSE.md).

## Diagnostics

There are two simple diagnostics, and each have their own code fixes.

`RADN0001` checks for `ValidateAntiForgeryTokenAttribute` in the presence of action methods decorated with:

* `HttpDeleteAttribute`
* `HttpPatchAttribute`
* `HttpPostAttribute`
* `HttpPutAttribute`

`RADN0002` checks for action methods that have neither `AllowAnonymousAttribute` nor `AuthorizeAttribute`.
An exception is made if the method's controller has `AuthorizeAttribute` but no exception is made for `AllowAnonymousAttribute`.
This means that `RADN0002`is opinionated: there should be (notionally) a `BaseController` with `AuthorizeAttribute`; authorization is implicit.
Anonymous actions must be decorated with `AllowAnonymousAttribute`; anonymous access is explicit.

## Miscellany

These diagnostics are only for ASP.NET MVC 3, 4 and 5.
