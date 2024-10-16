# About Package Development

The Package Development package lets you create and embed packages in Unity. It simplifies the process of developing and managing custom packages within Unity projects, enabling an efficient workflow for package management and customization.

# Installing Package Development

To install the **Package Development** package:

1. Open the **Package Manager** window (**Window** menu > **Package Manager**).
2. Click the "+" (plus) button.
3. Select **Add package by name**.
4. Enter the name: `com.unity.upm.develop`.
5. Click **Install** or **Add**, depending on your version of the Unity Editor.

# Using Package Development

## Create a package

1. Open the **Package Manager** window and click the "+" (plus) button.
2. Select **Create package** and enter a name for your package.
3. Click **Create** to start the creation of the new package.
   
## Select the custom package

After creating or embedding a package, choose the package from the **In Project** list in the **Package Manager** window to begin working with it.

## Use the Extensions buttons

After selecting a package, a set of extension buttons display in the details panel of the **Package Manager** window:

* **Test**: Run all Play mode and Edit mode tests for the currently selected package.
* **Validate**: Perform a validation check to ensure the package meets the required standards for publishing to the Asset Store.
* **Try-out**: Experiment with the package functionality in a new, temporary project in a separate Editor. This function lets you test your package without fully committing changes to it. It is especially useful to know if your package dependencies and samples are properly set.
* **Publish to disk**: Export the package to disk, creating a local copy that you can share or store for future use.

# Document revision history
 
|Date|Reason|
|---|---|
|October 16, 2024|Document modified. Matches package version 0.5.3-exp.1|
|March 29, 2019|Document created. Matches package version 0.1.|
