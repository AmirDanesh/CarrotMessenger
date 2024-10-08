/**
 * @name Async method naming
 * @description Finds async methods not ending with 'Async'.
 * @kind problem
 * @problem.severity warning
 * @id csharp/async-method-naming
 */

import csharp

from Method m
where m.hasModifier("async") and not m.getName().matches("%Async")
select m, "Async method does not end with 'Async'."
