#!/usr/local/bin/pwsh

# ================================
# Fixed constants (worst case)
# ================================

# Worst-case Stripe AMEX processing rate
$stripeRate = 0.035

# Australian GST rate
$gstRate = 0.10

# Desired profit margin as a percentage of airline fee
# Example: 0.05 = 5%
$profitMargin = 0.05


# ================================
# Single input
# ================================

# Airline fee charged by the airline
$airlineFee = 1279.91


# ================================
# Platform fee calculation
# ================================

$platformFee = (
    $airlineFee *
    (
        $profitMargin +
        (1 + $gstRate) * $stripeRate
    )
) / (
    1 -
    (1 + $gstRate) * (1 + $gstRate) * $stripeRate
)


# ================================
# Output
# ================================

$platformFee = [Math]::Round($platformFee, 2)
Write-Output "Platform booking fee to charge: ${platformFee}"
