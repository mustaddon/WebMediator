# pip install httpx
# pip install --upgrade setuptools wheel
# pip install --upgrade twine
# python setup.py sdist bdist_wheel
## python -m twine upload dist/webmediator-1.0.0.tar.gz


import setuptools


with open("README.md", "r") as fh:
    long_description = fh.read()

setuptools.setup(
    name="webmediator",
    version="1.0.0",
    author="Leonid Salavatov",
    author_email="mustaddon@gmail.com",
    description="Python sync/async client for the WebMediator API",
    long_description=long_description,
    long_description_content_type="text/markdown",
    url="https://github.com/mustaddon/webmediator.git",
    keywords=["WebMediator", "CQRS", "Mediator", "ApiClient"],
    packages=setuptools.find_packages(where='src'),
    package_dir={'': 'src'},
    classifiers=[
        "License :: OSI Approved :: MIT License",
        "Operating System :: OS Independent","Programming Language :: Python",
        "Programming Language :: Python :: 3",
        "Programming Language :: Python :: 3.8",
        "Programming Language :: Python :: 3.9",
        "Programming Language :: Python :: 3.10",
        "Programming Language :: Python :: 3.11",
        "Programming Language :: Python :: 3.12",
        "Programming Language :: Python :: 3.13",
        "Programming Language :: Python :: 3.14",
    ],
    install_requires=[ 'httpx' ],
    python_requires='>=3.8',
)