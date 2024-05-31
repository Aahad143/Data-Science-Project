def hello_world(*args):
    result = "Arguments received: " + ", ".join(str(arg) for arg in args)
    print(result)
    return result