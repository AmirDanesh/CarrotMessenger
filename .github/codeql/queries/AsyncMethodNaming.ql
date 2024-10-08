/**
 * @name Async method naming
 * @description Finds async methods not ending with 'Async'.
 * @kind problem
 * @problem.severity warning
 * @id csharp/async-method-naming
 * @tags clean-code
 */

import csharp

predicate isMediatRHandler(Method m) {
  exists(Type t |
    t = m.getDeclaringType() and
    t.getAnInterface().getName().matches("IRequestHandler%") or
    t.getAnInterface().getName().matches("INotificationHandler%")
  )
}

from Method m
where
  m.hasModifier("async") and
  not m.getName().matches("%Async") and
  not (m.getName() = "Handle" and isMediatRHandler(m))
select m, "Async method does not end with 'Async'."
