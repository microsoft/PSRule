# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

Export-PSRuleConvention 'AddModuleFiles' -Initialize {
    foreach ($inputFile in $PSRule.Repository.GetChangedFiles().WithExtension('.bicep')) {
        # Get module path
        $modulePath = $inputFile.AsFileInfo().Directory;
        while (!$modulePath.Name.StartsWith('v')) {
            $modulePath = $modulePath.Parent;
        }
        $moduleVersion = $modulePath.Name;
        $moduleName = $modulePath.Parent.Name;

        # Add whole module path to input files
        $PSRule.Input.Add($modulePath.FullName);

        # Add matching docs
        $PSRule.Input.Add("docs/modules/$moduleName-$moduleVersion/");
    }
}
