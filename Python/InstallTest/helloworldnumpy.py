import platform
import numpy as np

print("Python runtime version:", platform.python_version())
print("NumPy version:", np.__version__)

a = np.array([1, 2, 3])
b = np.array([10, 20, 30])

print("a =", a)
print("b =", b)
print("a + b =", a + b)
print("a * b =", a * b)
print("sum(a) =", np.sum(a))